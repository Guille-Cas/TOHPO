using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TOHPO.Data;
using TOHPO.Models;

namespace TOHPO.Pages.Operaciones.Ventas
{
    public class UpsertModel : PageModel
    {
        private readonly AppDbContext _context;

        public UpsertModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Venta Venta { get; set; } = new Venta();

        [BindProperty]
        public List<DetalleVentaViewModel> DetallesVenta { get; set; } = new List<DetalleVentaViewModel>();

        public SelectList ClientesList { get; set; } = default!;
        public List<Agente_Ventas> AgentesVentas { get; set; } = new List<Agente_Ventas>();
        public string AgenteSeleccionado { get; set; } = "";
        public List<Producto> ProductosDisponibles { get; set; } = new List<Producto>();

        public class DetalleVentaViewModel
        {
            public int Id { get; set; }
            public string CodigoProducto { get; set; } = string.Empty;
            public string NombreProducto { get; set; } = string.Empty;
            public int Cantidad { get; set; } = 1;
            public decimal PrecioUnitario { get; set; }
            public decimal PorcentajeDescuento { get; set; }
            public decimal MontoDescuento { get; set; }
            public decimal MontoImpuesto { get; set; }
            public decimal Subtotal { get; set; }
            public decimal PorcentajeImpuesto { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            await CargarDatos();

            if (id.HasValue)
            {
                var venta = await _context.Venta
                    .Include(v => v.Detalle_Ventas)
                        .ThenInclude(dv => dv.Producto)
                            .ThenInclude(p => p.Impuesto)
                    .Include(v => v.Agente_Ventas)
                    .FirstOrDefaultAsync(v => v.Id == id.Value);

                if (venta == null)
                {
                    TempData["ErrorMessage"] = "Venta no encontrada";
                    return RedirectToPage("./Index");
                }

                Venta = venta;
                AgenteSeleccionado = venta.Agente_Ventas?.Nombre ?? "";

                DetallesVenta = venta.Detalle_Ventas.Select(dv => new DetalleVentaViewModel
                {
                    Id = dv.Id,
                    CodigoProducto = dv.Codigo_Producto,
                    NombreProducto = dv.Producto.Descripcion,
                    Cantidad = dv.Cantidad,
                    PrecioUnitario = dv.Precio_Unitario,
                    PorcentajeDescuento = dv.Porcentaje_Descuento,
                    MontoDescuento = dv.Monto_Descuento,
                    MontoImpuesto = dv.Monto_Impuesto,
                    Subtotal = dv.Subtotal,
                    PorcentajeImpuesto = dv.Producto.Impuesto?.Porcentaje ?? 0
                }).ToList();
            }
            else
            {
                // Nueva venta
                Venta.Fecha = DateTime.Now.Date;
                Venta.Hora = DateTime.Now;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {

            ModelState.Remove("Venta.Cliente");
            ModelState.Remove("Venta.Agente_Ventas");
            ModelState.Remove("Venta.Concepto");
            
            if (!ModelState.IsValid)
            {
                await CargarDatos();
                return Page();
            }

            if (!DetallesVenta.Any())
            {
                ModelState.AddModelError("", "Debe agregar al menos un producto a la venta");
                await CargarDatos();
                return Page();
            }

            try
            {
                bool esNueva = Venta.Id == 0;

                // Validar inventario disponible antes de procesar la venta
                var validacionInventario = await ValidarInventarioDisponibleParaEdicion();
                if (!validacionInventario.esValido)
                {
                    ModelState.AddModelError("", validacionInventario.mensaje);
                    await CargarDatos();
                    return Page();
                }

                // Calcular totales
                CalcularTotales();

                if (esNueva)
                {
                    _context.Venta.Add(Venta);
                    await _context.SaveChangesAsync();

                    // Crear detalles y actualizar inventario
                    foreach (var detalle in DetallesVenta)
                    {
                        var detalleVenta = new Detalle_Venta
                        {
                            Id_Venta = Venta.Id,
                            Codigo_Producto = detalle.CodigoProducto,
                            Cantidad = detalle.Cantidad,
                            Precio_Unitario = detalle.PrecioUnitario,
                            Porcentaje_Descuento = detalle.PorcentajeDescuento,
                            Monto_Descuento = detalle.MontoDescuento,
                            Monto_Impuesto = detalle.MontoImpuesto,
                            Subtotal = detalle.Subtotal
                        };
                        _context.Detalle_Venta.Add(detalleVenta);

                        // Actualizar inventario
                        await ActualizarInventario(detalle.CodigoProducto, detalle.Cantidad, $"Venta #{Venta.Id}");
                    }

                    TempData["SuccessMessage"] = "Venta registrada exitosamente";
                }
                else
                {
                    // Para edición, necesitamos manejar los cambios de inventario más cuidadosamente
                    await ActualizarVentaExistente();
                    TempData["SuccessMessage"] = "Venta actualizada exitosamente";
                }

                await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al guardar la venta: " + ex.Message);
                await CargarDatos();
                return Page();
            }
        }

        private async Task<(bool esValido, string mensaje)> ValidarInventarioDisponible()
        {
            foreach (var detalle in DetallesVenta)
            {
                var inventario = await _context.Inventario
                    .Include(i => i.Producto)
                    .FirstOrDefaultAsync(i => i.Codigo_Producto == detalle.CodigoProducto);

                if (inventario == null)
                {
                    return (false, $"No se encontró inventario para el producto {detalle.CodigoProducto}");
                }

                if (inventario.Cantidad < detalle.Cantidad)
                {
                    return (false, $"Stock insuficiente para {inventario.Producto.Descripcion}. " +
                                  $"Disponible: {inventario.Cantidad}, Solicitado: {detalle.Cantidad}");
                }

                // Verificar que la cantidad no sea negativa después de la venta
                if (inventario.Cantidad - detalle.Cantidad < 0)
                {
                    return (false, $"La venta resultaría en stock negativo para {inventario.Producto.Descripcion}");
                }
            }

            return (true, "");
        }

        private async Task ActualizarInventario(string codigoProducto, int cantidadVendida, string motivo)
        {
            var inventario = await _context.Inventario
                .FirstOrDefaultAsync(i => i.Codigo_Producto == codigoProducto);

            if (inventario != null)
            {
                // Actualizar cantidad en inventario
                inventario.Cantidad -= cantidadVendida;
                inventario.Existencia = inventario.Cantidad; // Sincronizar existencia con cantidad
                
                _context.Inventario.Update(inventario);

                // Registrar movimiento de inventario
                var movimiento = new Movimiento_Inventario
                {
                    Id_Inventario = inventario.Id,
                    Cantidad = -cantidadVendida, // Negativo porque es una salida
                    Motivo = motivo,
                    Fecha = DateTime.Now
                };

                _context.Movimiento_Inventario.Add(movimiento);
            }
        }

        private async Task ActualizarVentaExistente()
        {
            // Obtener los detalles originales
            var detallesOriginales = await _context.Detalle_Venta
                .Where(dv => dv.Id_Venta == Venta.Id)
                .ToListAsync();

            // Crear un diccionario para manejar las diferencias
            var cambiosInventario = new Dictionary<string, int>();

            // Procesar productos originales
            foreach (var detalleOriginal in detallesOriginales)
            {
                if (!cambiosInventario.ContainsKey(detalleOriginal.Codigo_Producto))
                {
                    cambiosInventario[detalleOriginal.Codigo_Producto] = 0;
                }
                
                // Sumar la cantidad original (la devolvemos al inventario)
                cambiosInventario[detalleOriginal.Codigo_Producto] += detalleOriginal.Cantidad;
            }

            // Procesar productos nuevos/actualizados
            foreach (var detalleNuevo in DetallesVenta)
            {
                if (!cambiosInventario.ContainsKey(detalleNuevo.CodigoProducto))
                {
                    cambiosInventario[detalleNuevo.CodigoProducto] = 0;
                }
                
                // Restar la nueva cantidad (la sacamos del inventario)
                cambiosInventario[detalleNuevo.CodigoProducto] -= detalleNuevo.Cantidad;
            }

            // Aplicar cambios al inventario solo donde hay diferencias
            foreach (var cambio in cambiosInventario)
            {
                if (cambio.Value != 0) // Solo procesar si hay diferencia real
                {
                    var inventario = await _context.Inventario
                        .Include(i => i.Producto)
                        .FirstOrDefaultAsync(i => i.Codigo_Producto == cambio.Key);

                    if (inventario != null)
                    {
                        // Verificar que el cambio no deje el inventario en negativo
                        var nuevaCantidad = inventario.Cantidad + cambio.Value;
                        var nuevaExistencia = inventario.Existencia + cambio.Value;
                        
                        if (nuevaCantidad < 0 || nuevaExistencia < 0)
                        {
                            throw new Exception($"Stock insuficiente para {inventario.Producto.Descripcion}. " +
                                              $"Disponible: {inventario.Existencia}, cambio requerido: {cambio.Value * -1}");
                        }

                        // Aplicar el cambio
                        inventario.Cantidad = nuevaCantidad;
                        inventario.Existencia = nuevaExistencia;
                        _context.Inventario.Update(inventario);

                        // Registrar movimiento
                        string motivo;
                        if (cambio.Value > 0)
                        {
                            motivo = $"Ajuste positivo por edición de venta #{Venta.Id} (+{cambio.Value})";
                        }
                        else
                        {
                            motivo = $"Ajuste negativo por edición de venta #{Venta.Id} ({cambio.Value})";
                        }

                        var movimiento = new Movimiento_Inventario
                        {
                            Id_Inventario = inventario.Id,
                            Cantidad = cambio.Value,
                            Motivo = motivo,
                            Fecha = DateTime.Now
                        };
                        _context.Movimiento_Inventario.Add(movimiento);
                    }
                }
            }

            // Eliminar detalles originales
            _context.Detalle_Venta.RemoveRange(detallesOriginales);

            // Actualizar venta
            _context.Venta.Update(Venta);

            // Crear nuevos detalles
            foreach (var detalle in DetallesVenta)
            {
                var detalleVenta = new Detalle_Venta
                {
                    Id_Venta = Venta.Id,
                    Codigo_Producto = detalle.CodigoProducto,
                    Cantidad = detalle.Cantidad,
                    Precio_Unitario = detalle.PrecioUnitario,
                    Porcentaje_Descuento = detalle.PorcentajeDescuento,
                    Monto_Descuento = detalle.MontoDescuento,
                    Monto_Impuesto = detalle.MontoImpuesto,
                    Subtotal = detalle.Subtotal
                };
                _context.Detalle_Venta.Add(detalleVenta);
            }
        }

        private async Task<(bool esValido, string mensaje)> ValidarInventarioDisponibleParaEdicion()
        {
            if (Venta.Id == 0) // Nueva venta
            {
                return await ValidarInventarioDisponible();
            }

            // Para edición, validar considerando las diferencias
            var detallesOriginales = await _context.Detalle_Venta
                .Where(dv => dv.Id_Venta == Venta.Id)
                .ToListAsync();

            var cambiosInventario = new Dictionary<string, int>();

            // Calcular diferencias
            foreach (var detalleOriginal in detallesOriginales)
            {
                if (!cambiosInventario.ContainsKey(detalleOriginal.Codigo_Producto))
                {
                    cambiosInventario[detalleOriginal.Codigo_Producto] = 0;
                }
                cambiosInventario[detalleOriginal.Codigo_Producto] += detalleOriginal.Cantidad;
            }

            foreach (var detalleNuevo in DetallesVenta)
            {
                if (!cambiosInventario.ContainsKey(detalleNuevo.CodigoProducto))
                {
                    cambiosInventario[detalleNuevo.CodigoProducto] = 0;
                }
                cambiosInventario[detalleNuevo.CodigoProducto] -= detalleNuevo.Cantidad;
            }

            // Validar cada cambio
            foreach (var cambio in cambiosInventario)
            {
                if (cambio.Value < 0) // Solo validar si se necesita más inventario
                {
                    var inventario = await _context.Inventario
                        .Include(i => i.Producto)
                        .FirstOrDefaultAsync(i => i.Codigo_Producto == cambio.Key);

                    if (inventario == null)
                    {
                        return (false, $"No se encontró inventario para el producto {cambio.Key}");
                    }

                    var cantidadNecesaria = Math.Abs(cambio.Value);
                    if (inventario.Existencia < cantidadNecesaria)
                    {
                        return (false, $"Stock insuficiente para {inventario.Producto.Descripcion}. " +
                                      $"Disponible: {inventario.Existencia}, necesario: {cantidadNecesaria}");
                    }
                }
            }

            return (true, "");
        }

        private void CalcularTotales()
        {
            Venta.Costo_Total_Gravado = 0;
            Venta.Iva = 0;
            Venta.Total = 0;

            foreach (var detalle in DetallesVenta)
            {
                // Calcular monto de descuento
                detalle.MontoDescuento = (detalle.PrecioUnitario * detalle.Cantidad) * (detalle.PorcentajeDescuento / 100);

                // Subtotal sin impuesto
                var subtotalSinImpuesto = (detalle.PrecioUnitario * detalle.Cantidad) - detalle.MontoDescuento;

                // Calcular impuesto
                detalle.MontoImpuesto = subtotalSinImpuesto * (detalle.PorcentajeImpuesto / 100);

                // Subtotal final
                detalle.Subtotal = subtotalSinImpuesto + detalle.MontoImpuesto;

                // Acumular totales
                Venta.Costo_Total_Gravado += subtotalSinImpuesto;
                Venta.Iva += detalle.MontoImpuesto;
            }

            Venta.Total = Venta.Costo_Total_Gravado + Venta.Iva;
        }

        private async Task CargarDatos()
        {
            var clientes = await _context.Cliente
                .Where(c => c.Id > 0)
                .OrderBy(c => c.Nombre)
                .Select(c => new
                {
                    Id = c.Id,
                    Texto = $"{c.Nombre} {c.Primer_Apellido} {c.Segundo_Apellido} - {c.Cedula}"
                })
                .ToListAsync();

            ClientesList = new SelectList(clientes, "Id", "Texto");

            // Cargar todos los agentes activos para el modal
            AgentesVentas = await _context.Agente_Ventas
                .Include(a => a.Proveedor)
                .OrderBy(a => a.Nombre)
                .ToListAsync();

            ProductosDisponibles = await _context.Producto
                .Include(p => p.Impuesto)
                .Where(p => p.Estado)
                .OrderBy(p => p.Descripcion)
                .ToListAsync();
        }

        public async Task<JsonResult> OnGetProductoInfoAsync(string codigo)
        {
            var producto = await _context.Producto
                .Include(p => p.Impuesto)
                .FirstOrDefaultAsync(p => p.CodigoReferencia == codigo);

            if (producto == null)
            {
                return new JsonResult(new { success = false, message = "Producto no encontrado" });
            }

            // Obtener información del inventario
            var inventario = await _context.Inventario
                .FirstOrDefaultAsync(i => i.Codigo_Producto == codigo);

            if (inventario == null)
            {
                return new JsonResult(new { success = false, message = "Producto sin inventario disponible" });
            }

            if (inventario.Cantidad <= 0)
            {
                return new JsonResult(new { success = false, message = "Producto sin stock disponible" });
            }

            return new JsonResult(new
            {
                success = true,
                producto = new
                {
                    codigo = producto.CodigoReferencia,
                    nombre = producto.Descripcion,
                    precio = inventario.Precio_Venta,
                    porcentajeImpuesto = producto.Impuesto?.Porcentaje ?? 0,
                    stock = inventario.Cantidad
                }
            });
        }
    }
}