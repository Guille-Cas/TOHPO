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

        [BindProperty]
        public List<MetodoPagoViewModel> MetodosPago { get; set; } = new List<MetodoPagoViewModel>();

        public SelectList ClientesList { get; set; } = default!;
        public SelectList MetodosPagoList { get; set; } = default!;
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
                var venta = await _context.Venta
                    .Include(v => v.Detalle_Ventas)
                        .ThenInclude(dv => dv.Producto)
                            .ThenInclude(p => p.Impuesto)
                    .Include(v => v.Venta_Metodo_Pagos)
                        .ThenInclude(vmp => vmp.Metodo_Pago)
                    .FirstOrDefaultAsync(v => v.Id == id.Value);

                if (venta == null)
                {
                    TempData["ErrorMessage"] = "Venta no encontrada";
                    return RedirectToPage("./Index");
                }

                Venta = venta;

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

                MetodosPago = venta.Venta_Metodo_Pagos.Select(vmp => new MetodoPagoViewModel
                {
                    Id = vmp.Id,
                    IdMetodoPago = vmp.Id_Metodo_Pago,
                    NombreMetodoPago = vmp.Metodo_Pago.Descripcion,
                    Monto = vmp.Monto
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

        private async Task CargarDatos()
        {
            var clientes = await _context.Cliente
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ClientesList = new SelectList(clientes, "Id", "Nombre");

            var metodosPago = await _context.Metodo_Pago
                .OrderBy(mp => mp.Descripcion)
                .ToListAsync();

            MetodosPagoList = new SelectList(metodosPago, "Id", "Descripcion");

            ProductosDisponibles = await _context.Producto
                .Include(p => p.Inventario)
                .Include(p => p.Impuesto)
                .Where(p => p.Estado && p.Inventario != null && p.Inventario.Existencia > 0)
                .OrderBy(p => p.Descripcion)
                .ToListAsync();
        }

        public async Task<IActionResult> OnGetProductoInfoAsync(string codigo)
        {
            try
            {
                var producto = await _context.Producto
                    .Include(p => p.Inventario)
                    .Include(p => p.Impuesto)
                    .FirstOrDefaultAsync(p => p.CodigoReferencia == codigo && p.Estado);

                if (producto == null)
                {
                    return new JsonResult(new { success = false, message = "Producto no encontrado" });
                }

                if (producto.Inventario == null || producto.Inventario.Existencia <= 0)
                {
                    return new JsonResult(new { success = false, message = "Producto sin existencias" });
                }

                return new JsonResult(new
                {
                    success = true,
                    producto = new
                    {
                        codigo = producto.CodigoReferencia,
                        nombre = producto.Descripcion,
                        precio = producto.Inventario.Precio_Venta,
                        existencia = producto.Inventario.Existencia,
                        porcentajeImpuesto = producto.Impuesto?.Porcentaje ?? 0
                    }
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error al buscar el producto" });
            }
        }

        // NUEVO: Método para obtener productos para el modal
        public async Task<IActionResult> OnGetProductosInventarioAsync()
        {
            try
            {
                var productosConInventario = await _context.Inventario
                    .Include(i => i.Producto)
                        .ThenInclude(p => p.Impuesto)
                    .Where(i => i.Estado && i.Existencia > 0)
                    .Select(i => new
                    {
                        codigo = i.Codigo_Producto,
                        nombre = i.Producto.Descripcion,
                        cantidadInventario = i.Existencia,
                        precioUnitario = i.Precio_Venta,
                        porcentajeImpuesto = i.Producto.Impuesto != null ? i.Producto.Impuesto.Porcentaje : 0
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

        public async Task<IActionResult> OnPostAsync()
        {
            // Remover validaciones que no son necesarias
            ModelState.Remove("Venta.Cliente");
            ModelState.Remove("Venta.Detalle_Ventas");
            ModelState.Remove("Venta.Venta_Metodo_Pagos");
            ModelState.Remove("Venta.Id_Cliente");

            if (!ModelState.IsValid)
            {
                await CargarDatos();
                return Page();
            }

            if (!Venta.Id_Cliente.HasValue || Venta.Id_Cliente == 0)
            {
                Venta.Id_Cliente = null;
            }

            // Validar que hay productos en la venta
            if (DetallesVenta == null || !DetallesVenta.Any())
            {
                TempData["ErrorMessage"] = "Debe agregar al menos un producto a la venta";
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
            CalcularTotalesVenta();

            // Validar que los totales de métodos de pago coincidan con el total de la venta
            var totalMetodosPago = MetodosPago.Sum(mp => mp.Monto);
            var diferencia = Math.Abs(Venta.Total - totalMetodosPago);
            
            // Usar una tolerancia más pequeña y redondear a 2 decimales
            var totalVentaRedondeado = Math.Round(Venta.Total, 2);
            var totalPagosRedondeado = Math.Round(totalMetodosPago, 2);
            
            if (totalVentaRedondeado != totalPagosRedondeado)
            {
                TempData["ErrorMessage"] = $"El total de los métodos de pago (₡{totalPagosRedondeado:F2}) debe coincidir con el total de la venta (₡{totalVentaRedondeado:F2}). Diferencia: ₡{Math.Abs(totalVentaRedondeado - totalPagosRedondeado):F2}";
                await CargarDatos();
                return Page();
            }

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                if (Venta.Id == 0)
                {
                    // Nueva venta
                    Venta.Fecha=DateTime.Now.Date;
                    Venta.Hora = DateTime.Now;
                    await CrearNuevaVenta();
                }
                else
                {
                    // Actualizar venta existente
                    await ActualizarVentaExistente();
                }

                await transaction.CommitAsync();
                TempData["SuccessMessage"] = Venta.Id == 0 ? "Venta creada exitosamente" : "Venta actualizada exitosamente";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al procesar la venta: " + ex.Message;
                await CargarDatos();
                return Page();
            }
        }

        private async Task CrearNuevaVenta()
        {
            // Calcular totales
            CalcularTotalesVenta();

            // Agregar la venta
            _context.Venta.Add(Venta);
            await _context.SaveChangesAsync();

            // Agregar detalles de venta
            foreach (var detalle in DetallesVenta)
            {
                var detalleVenta = new Detalle_Venta
                {
                    Id_Venta = Venta.Id,
                    Codigo_Producto = detalle.CodigoProducto,
                    Cantidad = detalle.Cantidad,
                    Precio_Unitario = detalle.PrecioUnitario,
                    Porcentaje_Descuento = detalle.PorcentajeDescuento,
                    Monto_Descuento = CalcularMontoDescuento(detalle.PrecioUnitario, detalle.Cantidad, detalle.PorcentajeDescuento),
                    Monto_Impuesto = CalcularMontoImpuesto(detalle.PrecioUnitario, detalle.Cantidad, detalle.PorcentajeDescuento, detalle.PorcentajeImpuesto),
                    Subtotal = CalcularSubtotal(detalle.PrecioUnitario, detalle.Cantidad, detalle.PorcentajeDescuento)
                };

                _context.Detalle_Venta.Add(detalleVenta);

                // Actualizar inventario - reducir existencias
                var inventario = await _context.Inventario
                    .FirstOrDefaultAsync(i => i.Codigo_Producto == detalle.CodigoProducto);

                if (inventario != null)
                {
                    inventario.Existencia -= detalle.Cantidad;
                    _context.Inventario.Update(inventario);
                }
            }

            // Agregar métodos de pago
            foreach (var metodoPago in MetodosPago)
            {
                var ventaMetodoPago = new Venta_Metodo_Pago
                {
                    Id_Venta = Venta.Id,
                    Id_Metodo_Pago = metodoPago.IdMetodoPago,
                    Monto = metodoPago.Monto
                };

                _context.Venta_Metodo_Pago.Add(ventaMetodoPago);
            }

            await _context.SaveChangesAsync();
        }

        private async Task ActualizarVentaExistente()
        {
            // Obtener venta existente con sus detalles y métodos de pago
            var ventaExistente = await _context.Venta
                .Include(v => v.Detalle_Ventas)
                .Include(v => v.Venta_Metodo_Pagos)
                .FirstOrDefaultAsync(v => v.Id == Venta.Id);

            if (ventaExistente == null)
            {
                throw new InvalidOperationException("Venta no encontrada");
            }

            // Restaurar inventario de la venta original
            foreach (var detalleOriginal in ventaExistente.Detalle_Ventas)
            {
                var inventario = await _context.Inventario
                    .FirstOrDefaultAsync(i => i.Codigo_Producto == detalleOriginal.Codigo_Producto);

                if (inventario != null)
                {
                    inventario.Existencia += detalleOriginal.Cantidad;
                    _context.Inventario.Update(inventario);
                }
            }

            // Eliminar detalles y métodos de pago existentes
            _context.Detalle_Venta.RemoveRange(ventaExistente.Detalle_Ventas);
            _context.Venta_Metodo_Pago.RemoveRange(ventaExistente.Venta_Metodo_Pagos);

            // Actualizar datos de la venta
            ventaExistente.Fecha = Venta.Fecha;
            ventaExistente.Hora = Venta.Hora;
            ventaExistente.Concepto = Venta.Concepto;

            if (ventaExistente.Id_Cliente > 0)
            {
                ventaExistente.Id_Cliente = Venta.Id_Cliente;
            }

            // Calcular nuevos totales
            CalcularTotalesVenta();
            ventaExistente.Costo_Total_Gravado = Venta.Costo_Total_Gravado;
            ventaExistente.Iva = Venta.Iva;
            ventaExistente.Total = Venta.Total;

            _context.Venta.Update(ventaExistente);
            await _context.SaveChangesAsync();

            // Agregar nuevos detalles
            foreach (var detalle in DetallesVenta)
            {
                var detalleVenta = new Detalle_Venta
                {
                    Id_Venta = Venta.Id,
                    Codigo_Producto = detalle.CodigoProducto,
                    Cantidad = detalle.Cantidad,
                    Precio_Unitario = detalle.PrecioUnitario,
                    Porcentaje_Descuento = detalle.PorcentajeDescuento,
                    Monto_Descuento = CalcularMontoDescuento(detalle.PrecioUnitario, detalle.Cantidad, detalle.PorcentajeDescuento),
                    Monto_Impuesto = CalcularMontoImpuesto(detalle.PrecioUnitario, detalle.Cantidad, detalle.PorcentajeDescuento, detalle.PorcentajeImpuesto),
                    Subtotal = CalcularSubtotal(detalle.PrecioUnitario, detalle.Cantidad, detalle.PorcentajeDescuento)
                };

                _context.Detalle_Venta.Add(detalleVenta);

                // Actualizar inventario - reducir existencias
                var inventario = await _context.Inventario
                    .FirstOrDefaultAsync(i => i.Codigo_Producto == detalle.CodigoProducto);

                if (inventario != null)
                {
                    inventario.Existencia -= detalle.Cantidad;
                    _context.Inventario.Update(inventario);
                }
            }

            // Agregar nuevos métodos de pago
            foreach (var metodoPago in MetodosPago)
            {
                var ventaMetodoPago = new Venta_Metodo_Pago
                {
                    Id_Venta = Venta.Id,
                    Id_Metodo_Pago = metodoPago.IdMetodoPago,
                    Monto = metodoPago.Monto
                };

                _context.Venta_Metodo_Pago.Add(ventaMetodoPago);
            }

            await _context.SaveChangesAsync();
        }

        private void CalcularTotalesVenta()
        {
            decimal subtotalSinDescuento = 0;
            decimal totalDescuentos = 0;
            decimal totalImpuestos = 0;

            foreach (var detalle in DetallesVenta)
            {
                var subtotalLinea = detalle.PrecioUnitario * detalle.Cantidad;
                var descuentoLinea = CalcularMontoDescuento(detalle.PrecioUnitario, detalle.Cantidad, detalle.PorcentajeDescuento);
                var subtotalConDescuento = subtotalLinea - descuentoLinea;
                var impuestoLinea = CalcularMontoImpuesto(detalle.PrecioUnitario, detalle.Cantidad, detalle.PorcentajeDescuento, detalle.PorcentajeImpuesto);

                subtotalSinDescuento += subtotalLinea;
                totalDescuentos += descuentoLinea;
                totalImpuestos += impuestoLinea;
            }

            Venta.Costo_Total_Gravado = subtotalSinDescuento - totalDescuentos;
            Venta.Iva = totalImpuestos;
            Venta.Total = Venta.Costo_Total_Gravado + Venta.Iva;
        }

        private decimal CalcularMontoDescuento(decimal precioUnitario, int cantidad, decimal porcentajeDescuento)
        {
            var subtotal = precioUnitario * cantidad;
            return subtotal * (porcentajeDescuento / 100);
        }

        private decimal CalcularMontoImpuesto(decimal precioUnitario, int cantidad, decimal porcentajeDescuento, decimal porcentajeImpuesto)
        {
            var subtotal = precioUnitario * cantidad;
            var descuento = CalcularMontoDescuento(precioUnitario, cantidad, porcentajeDescuento);
            var subtotalConDescuento = subtotal - descuento;
            return subtotalConDescuento * (porcentajeImpuesto / 100);
        }

        private decimal CalcularSubtotal(decimal precioUnitario, int cantidad, decimal porcentajeDescuento)
        {
            var subtotal = precioUnitario * cantidad;
            var descuento = CalcularMontoDescuento(precioUnitario, cantidad, porcentajeDescuento);
            return subtotal - descuento;
        }
    }
}