using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TOHPO.Data;
using TOHPO.Models;

namespace TOHPO.Pages.Operaciones.Compras
{
    public class UpsertModel : PageModel
    {
        private readonly AppDbContext _context;

        public UpsertModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Compra Compra { get; set; } = new Compra();

        [BindProperty]
        public List<DetalleCompraViewModel> DetallesCompra { get; set; } = new List<DetalleCompraViewModel>();

        public SelectList ProveedoresList { get; set; } = default!;
        public List<Producto> ProductosDisponibles { get; set; } = new List<Producto>();

        public class DetalleCompraViewModel
        {
            public int Id { get; set; }
            public string CodigoProducto { get; set; } = string.Empty;
            public string NombreProducto { get; set; } = string.Empty;
            public int Cantidad { get; set; } = 1;
            public decimal CostoUnitario { get; set; }
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
                var compra = await _context.Compra
                    .Include(c => c.Compra_Detalles)
                        .ThenInclude(cd => cd.Producto)
                            .ThenInclude(p => p.Impuesto)
                    .FirstOrDefaultAsync(c => c.Id == id.Value);

                if (compra == null)
                {
                    TempData["ErrorMessage"] = "Compra no encontrada";
                    return RedirectToPage("./Index");
                }

                Compra = compra;

                DetallesCompra = compra.Compra_Detalles.Select(cd => new DetalleCompraViewModel
                {
                    Id = cd.Id,
                    CodigoProducto = cd.Codigo_Producto,
                    NombreProducto = cd.Producto.Descripcion,
                    Cantidad = cd.Cantidad,
                    CostoUnitario = cd.Costo_Unitario,
                    PorcentajeDescuento = cd.Porcentaje_Descuento,
                    MontoDescuento = cd.Monto_Descuento,
                    MontoImpuesto = cd.Monto_Impuesto,
                    Subtotal = cd.Subtotal,
                    PorcentajeImpuesto = cd.Producto.Impuesto?.Porcentaje ?? 0
                }).ToList();
            }
            else
            {
                // Nueva compra
                Compra.Fecha = DateTime.Now.Date;
                Compra.Hora = DateTime.Now;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Compra.Proveedor");
            ModelState.Remove("Compra.Concepto");
            
            if (!ModelState.IsValid)
            {
                await CargarDatos();
                return Page();
            }

            if (!DetallesCompra.Any())
            {
                ModelState.AddModelError("", "Debe agregar al menos un producto a la compra");
                await CargarDatos();
                return Page();
            }

            try
            {
                bool esNueva = Compra.Id == 0;

                // Validar que todos los productos estén registrados
                var validacionProductos = await ValidarProductosRegistrados();
                if (!validacionProductos.esValido)
                {
                    ModelState.AddModelError("", validacionProductos.mensaje);
                    await CargarDatos();
                    return Page();
                }

                // Calcular totales
                CalcularTotales();

                if (esNueva)
                {
                    _context.Compra.Add(Compra);
                    await _context.SaveChangesAsync();

                    // Crear detalles y actualizar inventario
                    foreach (var detalle in DetallesCompra)
                    {
                        var detalleCompra = new Compra_Detalle
                        {
                            Id_Compra = Compra.Id,
                            Codigo_Producto = detalle.CodigoProducto,
                            Cantidad = detalle.Cantidad,
                            Costo_Unitario = detalle.CostoUnitario,
                            Porcentaje_Descuento = detalle.PorcentajeDescuento,
                            Monto_Descuento = detalle.MontoDescuento,
                            Monto_Impuesto = detalle.MontoImpuesto,
                            Subtotal = detalle.Subtotal
                        };
                        _context.Compra_Detalle.Add(detalleCompra);

                        // Actualizar inventario
                        await ActualizarInventario(detalle.CodigoProducto, detalle.Cantidad, "ENTRADA", $"Compra #{Compra.Id}");
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Compra registrada exitosamente";
                }
                else
                {
                    // Editar compra existente usando el método diferencial
                    await ActualizarCompraExistente();
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Compra actualizada exitosamente";
                }

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al procesar la compra: {ex.Message}");
                await CargarDatos();
                return Page();
            }
        }

        public async Task<JsonResult> OnGetProductoInfoAsync(string codigo)
        {
            if (string.IsNullOrEmpty(codigo))
            {
                return new JsonResult(new { success = false, message = "Código de producto no válido" });
            }

            try
            {
                var producto = await _context.Producto
                    .Include(p => p.Impuesto)
                    .Include(p => p.Inventario) // Incluir inventario para obtener precio de compra
                    .FirstOrDefaultAsync(p => p.CodigoReferencia == codigo);

                if (producto == null)
                {
                    return new JsonResult(new { success = false, message = "Producto no encontrado en el catálogo" });
                }

                // Verificar si el producto tiene inventario registrado
                var inventario = await _context.Inventario
                    .FirstOrDefaultAsync(i => i.Codigo_Producto == codigo);

                if (inventario == null)
                {
                    return new JsonResult(new 
                    { 
                        success = false, 
                        message = "El producto no tiene inventario registrado. Debe crear primero un registro de inventario para este producto." 
                    });
                }

                // Determinar el costo unitario sugerido
                decimal costoUnitarioSugerido = 0;
                
                // Prioridad 1: Precio de compra actual en inventario (si existe y es mayor a 0)
                if (inventario.Precio_Compra > 0)
                {
                    costoUnitarioSugerido = inventario.Precio_Compra;
                }
                // Prioridad 2: Buscar la última compra de este producto
                else
                {
                    var ultimaCompra = await _context.Compra_Detalle
                        .Where(cd => cd.Codigo_Producto == codigo)
                        .Include(cd => cd.Compra)
                        .OrderByDescending(cd => cd.Compra.Fecha)
                        .ThenByDescending(cd => cd.Compra.Hora)
                        .FirstOrDefaultAsync();

                    if (ultimaCompra != null)
                    {
                        costoUnitarioSugerido = ultimaCompra.Costo_Unitario;
                    }
                    // Si no hay histórico, dejar en 0 para que el usuario ingrese el precio
                }

                var productoInfo = new
                {
                    codigo = producto.CodigoReferencia,
                    nombre = producto.Descripcion,
                    porcentajeImpuesto = producto.Impuesto?.Porcentaje ?? 0,
                    cantidadTotal = inventario.Cantidad,
                    existenciaDisponible = inventario.Existencia,
                    costoUnitarioSugerido = costoUnitarioSugerido,
                    precioCompraActual = inventario.Precio_Compra,
                    tieneHistorialCompras = costoUnitarioSugerido > 0
                };

                return new JsonResult(new { success = true, producto = productoInfo });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al obtener información del producto: {ex.Message}" });
            }
        }

        private async Task CargarDatos()
        {
            // Cargar proveedores activos
            var proveedores = await _context.Proveedor
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            ProveedoresList = new SelectList(proveedores, "Id", "Nombre");

            // Cargar productos disponibles
            ProductosDisponibles = await _context.Producto
                .Include(p => p.Impuesto)
                .Where(p => p.Estado == true)
                .OrderBy(p => p.Descripcion)
                .ToListAsync();
        }

        private async Task<(bool esValido, string mensaje)> ValidarProductosRegistrados()
        {
            foreach (var detalle in DetallesCompra)
            {
                var producto = await _context.Producto
                    .FirstOrDefaultAsync(p => p.CodigoReferencia == detalle.CodigoProducto);

                if (producto == null)
                {
                    return (false, $"El producto con código {detalle.CodigoProducto} no está registrado en el catálogo.");
                }

                // Verificar que el producto tenga inventario registrado
                var inventario = await _context.Inventario
                    .FirstOrDefaultAsync(i => i.Codigo_Producto == detalle.CodigoProducto);

                if (inventario == null)
                {
                    return (false, $"El producto {producto.Descripcion} no tiene inventario registrado. Debe crear primero un registro de inventario.");
                }
            }

            return (true, "");
        }

        private void CalcularTotales()
        {
            decimal subtotal = 0;
            decimal totalDescuentos = 0;
            decimal totalIva = 0;

            foreach (var detalle in DetallesCompra)
            {
                // Calcular monto de descuento
                detalle.MontoDescuento = (detalle.CostoUnitario * detalle.Cantidad) * (detalle.PorcentajeDescuento / 100);

                // Subtotal sin impuesto
                decimal subtotalSinImpuesto = (detalle.CostoUnitario * detalle.Cantidad) - detalle.MontoDescuento;

                // Calcular impuesto
                detalle.MontoImpuesto = subtotalSinImpuesto * (detalle.PorcentajeImpuesto / 100);

                // Subtotal final del producto
                detalle.Subtotal = subtotalSinImpuesto + detalle.MontoImpuesto;

                // Acumular
                subtotal += subtotalSinImpuesto;
                totalDescuentos += detalle.MontoDescuento;
                totalIva += detalle.MontoImpuesto;
            }

            Compra.Costo_Total_Grabado = subtotal;
            Compra.Iva = totalIva;
            Compra.Total = subtotal + totalIva;
        }

        private async Task ActualizarInventario(string codigoProducto, int cantidad, string tipoMovimiento, string concepto)
        {
            try
            {
                // Buscar el inventario del producto
                var inventario = await _context.Inventario
                    .FirstOrDefaultAsync(i => i.Codigo_Producto == codigoProducto);

                if (inventario == null)
                {
                    throw new Exception($"No se encontró inventario para el producto {codigoProducto}");
                }

                // Actualizar tanto Cantidad como Existencia de manera coherente
                // Para compras (ENTRADA): se aumentan ambos valores
                if (tipoMovimiento == "ENTRADA")
                {
                    inventario.Cantidad += cantidad;      // Cantidad total acumulada
                    inventario.Existencia += cantidad;    // Existencia disponible actual
                }
                else if (tipoMovimiento == "SALIDA")
                {
                    inventario.Cantidad -= cantidad;      // Cantidad total
                    inventario.Existencia -= cantidad;    // Existencia disponible

                    // Validar que no queden valores negativos
                    if (inventario.Cantidad < 0)
                    {
                        throw new Exception($"La cantidad total del producto {codigoProducto} no puede ser negativa.");
                    }
                    
                    if (inventario.Existencia < 0)
                    {
                        throw new Exception($"La existencia del producto {codigoProducto} no puede ser negativa.");
                    }
                }

                // Actualizar precio de compra si es una entrada
                if (tipoMovimiento == "ENTRADA" && cantidad > 0)
                {
                    // Buscar el detalle de compra para obtener el costo unitario
                    var detalleCompra = DetallesCompra.FirstOrDefault(d => d.CodigoProducto == codigoProducto);
                    if (detalleCompra != null)
                    {
                        inventario.Precio_Compra = detalleCompra.CostoUnitario;
                    }
                }

                // Crear movimiento de inventario para auditoría
                var movimiento = new Movimiento_Inventario
                {
                    Id_Inventario = inventario.Id,
                    Cantidad = cantidad,
                    Motivo = concepto,
                    Fecha = DateTime.Now
                };

                _context.Movimiento_Inventario.Add(movimiento);
                _context.Update(inventario);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar inventario para producto {codigoProducto}: {ex.Message}");
            }
        }

        private async Task ActualizarCompraExistente()
        {
            // Obtener los detalles originales
            var detallesOriginales = await _context.Compra_Detalle
                .Where(cd => cd.Id_Compra == Compra.Id)
                .ToListAsync();

            // Crear un diccionario para manejar las diferencias
            var cambiosInventario = new Dictionary<string, (int cantidad, decimal costoUnitario)>();

            // Procesar productos originales (los restamos porque los "devolvemos" del inventario)
            foreach (var detalleOriginal in detallesOriginales)
            {
                if (!cambiosInventario.ContainsKey(detalleOriginal.Codigo_Producto))
                {
                    cambiosInventario[detalleOriginal.Codigo_Producto] = (0, 0);
                }
                
                // Restar la cantidad original (la sacamos del inventario)
                var actual = cambiosInventario[detalleOriginal.Codigo_Producto];
                cambiosInventario[detalleOriginal.Codigo_Producto] = (actual.cantidad - detalleOriginal.Cantidad, actual.costoUnitario);
            }

            // Procesar productos nuevos/actualizados (los sumamos al inventario)
            foreach (var detalleNuevo in DetallesCompra)
            {
                if (!cambiosInventario.ContainsKey(detalleNuevo.CodigoProducto))
                {
                    cambiosInventario[detalleNuevo.CodigoProducto] = (0, 0);
                }
                
                // Sumar la nueva cantidad (la agregamos al inventario)
                var actual = cambiosInventario[detalleNuevo.CodigoProducto];
                cambiosInventario[detalleNuevo.CodigoProducto] = (actual.cantidad + detalleNuevo.Cantidad, detalleNuevo.CostoUnitario);
            }

            // Aplicar cambios al inventario solo donde hay diferencias
            foreach (var cambio in cambiosInventario)
            {
                if (cambio.Value.cantidad != 0) // Solo procesar si hay diferencia real
                {
                    var inventario = await _context.Inventario
                        .Include(i => i.Producto)
                        .FirstOrDefaultAsync(i => i.Codigo_Producto == cambio.Key);

                    if (inventario != null)
                    {
                        // Verificar que el cambio no deje el inventario en negativo
                        var nuevaCantidad = inventario.Cantidad + cambio.Value.cantidad;
                        var nuevaExistencia = inventario.Existencia + cambio.Value.cantidad;
                        
                        if (nuevaCantidad < 0 || nuevaExistencia < 0)
                        {
                            throw new Exception($"No se puede reducir el inventario de {inventario.Producto.Descripcion}. " +
                                              $"Disponible: {inventario.Existencia}, reducción requerida: {Math.Abs(cambio.Value.cantidad)}");
                        }

                        // Aplicar el cambio
                        inventario.Cantidad = nuevaCantidad;
                        inventario.Existencia = nuevaExistencia;
                        
                        // Actualizar precio de compra si hay cantidad positiva
                        if (cambio.Value.cantidad > 0 && cambio.Value.costoUnitario > 0)
                        {
                            inventario.Precio_Compra = cambio.Value.costoUnitario;
                        }
                        
                        _context.Inventario.Update(inventario);

                        // Registrar movimiento
                        string motivo;
                        
                        if (cambio.Value.cantidad > 0)
                        {
                            motivo = $"Ajuste positivo por edición de compra #{Compra.Id} (+{cambio.Value.cantidad})";
                        }
                        else
                        {
                            motivo = $"Ajuste negativo por edición de compra #{Compra.Id} ({cambio.Value.cantidad})";
                        }

                        var movimiento = new Movimiento_Inventario
                        {
                            Id_Inventario = inventario.Id,
                            Cantidad = Math.Abs(cambio.Value.cantidad),
                            Motivo = motivo,
                            Fecha = DateTime.Now
                        };
                        _context.Movimiento_Inventario.Add(movimiento);
                    }
                }
            }

            // Actualizar propiedades de la compra
            var compraExistente = await _context.Compra.FirstOrDefaultAsync(c => c.Id == Compra.Id);
            if (compraExistente != null)
            {
                compraExistente.Fecha = Compra.Fecha;
                compraExistente.Hora = Compra.Hora;
                compraExistente.Id_Proveedor = Compra.Id_Proveedor;
                compraExistente.Concepto = Compra.Concepto;
                compraExistente.Costo_Total_Grabado = Compra.Costo_Total_Grabado;
                compraExistente.Iva = Compra.Iva;
                compraExistente.Total = Compra.Total;
            }

            // Eliminar detalles originales
            _context.Compra_Detalle.RemoveRange(detallesOriginales);

            // Crear nuevos detalles
            foreach (var detalle in DetallesCompra)
            {
                var detalleCompra = new Compra_Detalle
                {
                    Id_Compra = Compra.Id,
                    Codigo_Producto = detalle.CodigoProducto,
                    Cantidad = detalle.Cantidad,
                    Costo_Unitario = detalle.CostoUnitario,
                    Porcentaje_Descuento = detalle.PorcentajeDescuento,
                    Monto_Descuento = detalle.MontoDescuento,
                    Monto_Impuesto = detalle.MontoImpuesto,
                    Subtotal = detalle.Subtotal
                };
                _context.Compra_Detalle.Add(detalleCompra);
            }
        }
    }
}