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

        [BindProperty]
        public List<MetodoPagoViewModel> MetodosPago { get; set; } = new List<MetodoPagoViewModel>();

        public SelectList ProveedoresList { get; set; } = default!;
        public SelectList MetodosPagoList { get; set; } = default!;
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

        public class MetodoPagoViewModel
        {
            public int Id { get; set; }
            public int IdMetodoPago { get; set; }
            public string NombreMetodoPago { get; set; } = string.Empty;
            public decimal Monto { get; set; }
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
                    .Include(c => c.Compra_Metodo_Pagos)
                        .ThenInclude(cmp => cmp.Metodo_Pago)
                    .FirstOrDefaultAsync(c => c.Id == id.Value);

                if (compra == null)
                {
                    TempData["ErrorMessage"] = "Compra no encontrada";
                    return RedirectToPage("./Index");
                }

                Compra = compra;

                // Cargar detalles para edición
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

                // CORRECCIÓN: Cargar métodos de pago para edición - esta línea estaba correcta
                MetodosPago = compra.Compra_Metodo_Pagos.Select(cmp => new MetodoPagoViewModel
                {
                    Id = cmp.Id,
                    IdMetodoPago = cmp.Id_Metodo_Pago,
                    NombreMetodoPago = cmp.Metodo_Pago.Descripcion,
                    Monto = cmp.Monto
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
            // Remover validaciones que no son necesarias
            ModelState.Remove("Compra.Proveedor");
            ModelState.Remove("Compra.Compra_Detalles");
            ModelState.Remove("Compra.Compra_Metodo_Pagos");
            
            if (!ModelState.IsValid)
            {
                await CargarDatos();
                return Page();
            }

            // Validar que hay productos en la compra
            if (DetallesCompra == null || !DetallesCompra.Any())
            {
                TempData["ErrorMessage"] = "Debe agregar al menos un producto a la compra";
                await CargarDatos();
                return Page();
            }

            // Validar que hay métodos de pago
            if (MetodosPago == null || !MetodosPago.Any())
            {
                TempData["ErrorMessage"] = "Debe agregar al menos un método de pago";
                await CargarDatos();
                return Page();
            }

            // Calcular totales antes de la validación
            CalcularTotales();

            // NUEVA LÓGICA: Validar métodos de pago - permitir montos superiores para flujo de caja
            var totalMetodosPago = MetodosPago.Sum(mp => mp.Monto);
            var totalCompraRedondeado = Math.Round(Compra.Total, 2);
            var totalPagosRedondeado = Math.Round(totalMetodosPago, 2);
            
            // Validar que el total de pagos no sea menor que el total de la compra
            if (totalPagosRedondeado < totalCompraRedondeado)
            {
                var diferencia = totalCompraRedondeado - totalPagosRedondeado;
                TempData["ErrorMessage"] = $"El total de los métodos de pago (₡{totalPagosRedondeado:F2}) no puede ser menor que el total de la compra (₡{totalCompraRedondeado:F2}). Faltante: ₡{diferencia:F2}";
                await CargarDatos();
                return Page();
            }
            
            // Si el pago es mayor que la compra, mostrar información sobre el pago adelantado
            if (totalPagosRedondeado > totalCompraRedondeado)
            {
                var exceso = totalPagosRedondeado - totalCompraRedondeado;
                TempData["InfoMessage"] = $"Pago realizado: ₡{totalPagosRedondeado:F2} | Total compra: ₡{totalCompraRedondeado:F2} | Pago adelantado/exceso: ₡{exceso:F2}";
            }

            try
            {
                // Validar que todos los productos estén registrados
                var validacionProductos = await ValidarProductosRegistrados();
                if (!validacionProductos.esValido)
                {
                    TempData["ErrorMessage"] = validacionProductos.mensaje;
                    await CargarDatos();
                    return Page();
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                // CORRECCIÓN: Verificar si es nueva o edición basándose en si existe el ID en la base de datos
                var compraExistente = await _context.Compra.FirstOrDefaultAsync(c => c.Id == Compra.Id);
                
                if (compraExistente == null)
                {
                    // Nueva compra - si no existe en la BD, es nueva
                    await CrearNuevaCompra();
                }
                else
                {
                    // Actualizar compra existente - si existe en la BD, es edición
                    await ActualizarCompraExistente();
                }

                await transaction.CommitAsync();
                TempData["SuccessMessage"] = compraExistente == null ? "Compra creada exitosamente" : "Compra actualizada exitosamente";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al procesar la compra: " + ex.Message;
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
                    .Include(p => p.Inventario)
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
                
                if (inventario.Precio_Compra > 0)
                {
                    costoUnitarioSugerido = inventario.Precio_Compra;
                }
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
                }

                var productoInfo = new
                {
                    codigo = producto.CodigoReferencia,
                    nombre = producto.Descripcion,
                    costo = costoUnitarioSugerido,
                    porcentajeImpuesto = producto.Impuesto?.Porcentaje ?? 0
                };

                return new JsonResult(new { success = true, producto = productoInfo });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al obtener información del producto: {ex.Message}" });
            }
        }

        private async Task CrearNuevaCompra()
        {
            // Calcular totales
            CalcularTotales();

            // Agregar la compra
            _context.Compra.Add(Compra);
            await _context.SaveChangesAsync();

            // Agregar detalles de compra
            foreach (var detalle in DetallesCompra)
            {
                var detalleCompra = new Compra_Detalle
                {
                    Id_Compra = Compra.Id,
                    Codigo_Producto = detalle.CodigoProducto,
                    Cantidad = detalle.Cantidad,
                    Costo_Unitario = detalle.CostoUnitario,
                    Porcentaje_Descuento = detalle.PorcentajeDescuento,
                    Monto_Descuento = CalcularMontoDescuento(detalle.CostoUnitario, detalle.Cantidad, detalle.PorcentajeDescuento),
                    Monto_Impuesto = CalcularMontoImpuesto(detalle.CostoUnitario, detalle.Cantidad, detalle.PorcentajeDescuento, detalle.PorcentajeImpuesto),
                    Subtotal = CalcularSubtotal(detalle.CostoUnitario, detalle.Cantidad, detalle.PorcentajeDescuento)
                };

                _context.Compra_Detalle.Add(detalleCompra);

                // Actualizar inventario - aumentar existencias
                await ActualizarInventario(detalle.CodigoProducto, detalle.Cantidad, "ENTRADA", $"Compra #{Compra.Id}");
            }

            // Agregar métodos de pago
            foreach (var metodoPago in MetodosPago)
            {
                var compraMetodoPago = new Compra_Metodo_Pago
                {
                    Id_Compra = Compra.Id,
                    Id_Metodo_Pago = metodoPago.IdMetodoPago,
                    Monto = metodoPago.Monto
                };

                _context.Compra_Metodo_Pago.Add(compraMetodoPago);
            }

            await _context.SaveChangesAsync();
        }

        private async Task ActualizarCompraExistente()
        {
            // Obtener compra existente con sus detalles y métodos de pago
            var compraExistente = await _context.Compra
                .Include(c => c.Compra_Detalles)
                .Include(c => c.Compra_Metodo_Pagos)
                .FirstOrDefaultAsync(c => c.Id == Compra.Id);

            if (compraExistente == null)
            {
                throw new InvalidOperationException("Compra no encontrada");
            }

            // Restaurar inventario de la compra original
            foreach (var detalleOriginal in compraExistente.Compra_Detalles)
            {
                await ActualizarInventario(detalleOriginal.Codigo_Producto, detalleOriginal.Cantidad, "SALIDA", $"Reversión edición compra #{Compra.Id}");
            }

            // Eliminar detalles y métodos de pago existentes
            _context.Compra_Detalle.RemoveRange(compraExistente.Compra_Detalles);
            _context.Compra_Metodo_Pago.RemoveRange(compraExistente.Compra_Metodo_Pagos);

            // Actualizar datos de la compra
            compraExistente.Fecha = Compra.Fecha;
            compraExistente.Hora = Compra.Hora;
            compraExistente.Id_Proveedor = Compra.Id_Proveedor;
            compraExistente.Concepto = Compra.Concepto;

            // Calcular nuevos totales
            CalcularTotales();
            compraExistente.Costo_Total_Grabado = Compra.Costo_Total_Grabado;
            compraExistente.Iva = Compra.Iva;
            compraExistente.Total = Compra.Total;

            _context.Compra.Update(compraExistente);
            await _context.SaveChangesAsync();

            // Agregar nuevos detalles
            foreach (var detalle in DetallesCompra)
            {
                var detalleCompra = new Compra_Detalle
                {
                    Id_Compra = Compra.Id,
                    Codigo_Producto = detalle.CodigoProducto,
                    Cantidad = detalle.Cantidad,
                    Costo_Unitario = detalle.CostoUnitario,
                    Porcentaje_Descuento = detalle.PorcentajeDescuento,
                    Monto_Descuento = CalcularMontoDescuento(detalle.CostoUnitario, detalle.Cantidad, detalle.PorcentajeDescuento),
                    Monto_Impuesto = CalcularMontoImpuesto(detalle.CostoUnitario, detalle.Cantidad, detalle.PorcentajeDescuento, detalle.PorcentajeImpuesto),
                    Subtotal = CalcularSubtotal(detalle.CostoUnitario, detalle.Cantidad, detalle.PorcentajeDescuento)
                };

                _context.Compra_Detalle.Add(detalleCompra);

                // Actualizar inventario - aumentar existencias
                await ActualizarInventario(detalle.CodigoProducto, detalle.Cantidad, "ENTRADA", $"Compra #{Compra.Id}");
            }

            // Agregar nuevos métodos de pago
            foreach (var metodoPago in MetodosPago)
            {
                var compraMetodoPago = new Compra_Metodo_Pago
                {
                    Id_Compra = Compra.Id,
                    Id_Metodo_Pago = metodoPago.IdMetodoPago,
                    Monto = metodoPago.Monto
                };

                _context.Compra_Metodo_Pago.Add(compraMetodoPago);
            }

            await _context.SaveChangesAsync();
        }

        private async Task CargarDatos()
        {
            // Cargar proveedores activos
            var proveedores = await _context.Proveedor
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            ProveedoresList = new SelectList(proveedores, "Id", "Nombre");

            // Cargar métodos de pago
            var metodosPago = await _context.Metodo_Pago
                .OrderBy(mp => mp.Descripcion)
                .ToListAsync();

            MetodosPagoList = new SelectList(metodosPago, "Id", "Descripcion");

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
            decimal subtotalSinDescuento = 0;
            decimal totalDescuentos = 0;
            decimal totalImpuestos = 0;

            foreach (var detalle in DetallesCompra)
            {
                var subtotalLinea = detalle.CostoUnitario * detalle.Cantidad;
                var descuentoLinea = CalcularMontoDescuento(detalle.CostoUnitario, detalle.Cantidad, detalle.PorcentajeDescuento);
                var subtotalConDescuento = subtotalLinea - descuentoLinea;
                var impuestoLinea = CalcularMontoImpuesto(detalle.CostoUnitario, detalle.Cantidad, detalle.PorcentajeDescuento, detalle.PorcentajeImpuesto);

                subtotalSinDescuento += subtotalLinea;
                totalDescuentos += descuentoLinea;
                totalImpuestos += impuestoLinea;
            }

            Compra.Costo_Total_Grabado = subtotalSinDescuento - totalDescuentos;
            Compra.Iva = totalImpuestos;
            Compra.Total = Compra.Costo_Total_Grabado + Compra.Iva;
        }

        private decimal CalcularMontoDescuento(decimal costoUnitario, int cantidad, decimal porcentajeDescuento)
        {
            var subtotal = costoUnitario * cantidad;
            return subtotal * (porcentajeDescuento / 100);
        }

        private decimal CalcularMontoImpuesto(decimal costoUnitario, int cantidad, decimal porcentajeDescuento, decimal porcentajeImpuesto)
        {
            var subtotal = costoUnitario * cantidad;
            var descuento = CalcularMontoDescuento(costoUnitario, cantidad, porcentajeDescuento);
            var subtotalConDescuento = subtotal - descuento;
            return subtotalConDescuento * (porcentajeImpuesto / 100);
        }

        private decimal CalcularSubtotal(decimal costoUnitario, int cantidad, decimal porcentajeDescuento)
        {
            var subtotal = costoUnitario * cantidad;
            var descuento = CalcularMontoDescuento(costoUnitario, cantidad, porcentajeDescuento);
            return subtotal - descuento;
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

                // Actualizar inventario según tipo de movimiento
                if (tipoMovimiento == "ENTRADA")
                {
                    inventario.Cantidad += cantidad;
                    inventario.Existencia += cantidad;
                    
                    // Actualizar precio de compra
                    var detalleCompra = DetallesCompra.FirstOrDefault(d => d.CodigoProducto == codigoProducto);
                    if (detalleCompra != null)
                    {
                        inventario.Precio_Compra = detalleCompra.CostoUnitario;
                    }
                }
                else if (tipoMovimiento == "SALIDA")
                {
                    inventario.Cantidad -= cantidad;
                    inventario.Existencia -= cantidad;

                    // Validar que no queden valores negativos
                    if (inventario.Cantidad < 0 || inventario.Existencia < 0)
                    {
                        throw new Exception($"La cantidad del producto {codigoProducto} no puede ser negativa.");
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

        public async Task<IActionResult> OnGetProductosInventarioAsync()
        {
            try
            {
                // Obtener productos con información de inventario
                var productosConInventario = await _context.Producto
                    .Include(p => p.Impuesto)
                    .Where(p => p.Estado == true)
                    .Select(p => new
                    {
                        codigo = p.CodigoReferencia,
                        nombre = p.Descripcion,
                        descripcion = p.Descripcion,
                        costo = _context.Inventario
                            .Where(i => i.Codigo_Producto == p.CodigoReferencia)
                            .Select(i => i.Precio_Compra)
                            .FirstOrDefault() > 0 ? _context.Inventario
                            .Where(i => i.Codigo_Producto == p.CodigoReferencia)
                            .Select(i => i.Precio_Compra)
                            .FirstOrDefault() : 0,
                        cantidadInventario = _context.Inventario
                            .Where(i => i.Codigo_Producto == p.CodigoReferencia)
                            .Sum(i => i.Existencia),
                        porcentajeImpuesto = p.Impuesto != null ? p.Impuesto.Porcentaje : 0
                    })
                    .OrderBy(p => p.nombre)
                    .ToListAsync();

                return new JsonResult(new { success = true, productos = productosConInventario });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error al cargar productos: " + ex.Message });
            }
        }
    }
}