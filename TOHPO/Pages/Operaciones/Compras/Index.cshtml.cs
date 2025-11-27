using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TOHPO.Data;
using TOHPO.Models;

namespace TOHPO.Pages.Operaciones.Compras
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Compra> Compras { get; set; } = default!;

        [BindProperty]
        public DateTime? FechaInicio { get; set; }

        [BindProperty]
        public DateTime? FechaFin { get; set; }

        [BindProperty]
        public string? BuscarProveedor { get; set; }

        public async Task OnGetAsync()
        {
            await CargarCompras();
        }

        public async Task<IActionResult> OnPostFiltrarAsync()
        {
            await CargarCompras();
            return Page();
        }

        private async Task CargarCompras()
        {
            if (_context.Compra != null)
            {
                var query = _context.Compra
                    .Include(c => c.Proveedor)
                    .Include(c => c.Compra_Detalles)
                        .ThenInclude(cd => cd.Producto)
                    .AsQueryable();

                // Filtros
                if (FechaInicio.HasValue)
                {
                    query = query.Where(c => c.Fecha >= FechaInicio.Value);
                }

                if (FechaFin.HasValue)
                {
                    query = query.Where(c => c.Fecha <= FechaFin.Value);
                }

                if (!string.IsNullOrEmpty(BuscarProveedor))
                {
                    query = query.Where(c => c.Proveedor.Nombre.Contains(BuscarProveedor) ||
                                           c.Proveedor.Correo_Electronico.Contains(BuscarProveedor));
                }

                Compras = await query
                    .OrderByDescending(c => c.Fecha)
                    .ThenByDescending(c => c.Hora)
                    .ToListAsync();
            }
        }

        public async Task<IActionResult> OnGetEliminarAsync(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "ID de compra no válido";
                return RedirectToPage();
            }

            var compra = await _context.Compra
                .Include(c => c.Compra_Detalles)
                .Include(c => c.Compra_Metodo_Pagos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (compra == null)
            {
                TempData["ErrorMessage"] = "Compra no encontrada";
                return RedirectToPage();
            }

            try
            {
                // IMPORTANTE: NO afectamos el inventario al eliminar registros de compra
                // La eliminación de registros es solo administrativa/contable
                // El inventario se mantiene tal como estaba

                // Registrar la eliminación en los movimientos para auditoría (opcional)
                foreach (var detalle in compra.Compra_Detalles)
                {
                    var inventario = await _context.Inventario
                        .FirstOrDefaultAsync(i => i.Codigo_Producto == detalle.Codigo_Producto);

                    if (inventario != null)
                    {
                        // Solo registrar el movimiento para auditoría, sin cambiar cantidades
                        var movimiento = new Movimiento_Inventario
                        {
                            Id_Inventario = inventario.Id,
                            Cantidad = 0, // Cantidad 0 indica que es solo informativo
                            Motivo = $"Eliminación de registro - Compra #{compra.Id} (Sin afectar inventario)",
                            Fecha = DateTime.Now
                        };
                        _context.Movimiento_Inventario.Add(movimiento);
                    }
                }

                // Eliminar detalles de compra
                if (compra.Compra_Detalles.Any())
                {
                    _context.Compra_Detalle.RemoveRange(compra.Compra_Detalles);
                }

                // Eliminar métodos de pago
                if (compra.Compra_Metodo_Pagos.Any())
                {
                    _context.Compra_Metodo_Pago.RemoveRange(compra.Compra_Metodo_Pagos);
                }

                // Eliminar la compra
                _context.Compra.Remove(compra);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Registro de compra eliminado exitosamente. El inventario no se ha visto afectado.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar el registro de compra: " + ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<JsonResult> OnGetDetalleCompraAsync(int id)
        {
            var compra = await _context.Compra
                .Include(c => c.Proveedor)
                .Include(c => c.Compra_Detalles)
                    .ThenInclude(cd => cd.Producto)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (compra == null)
            {
                return new JsonResult(new { success = false, message = "Compra no encontrada" });
            }

            var detalle = new
            {
                success = true,
                compra = new
                {
                    id = compra.Id,
                    fecha = compra.Fecha.ToString("dd/MM/yyyy"),
                    hora = compra.Hora.ToString("HH:mm"),
                    proveedor = compra.Proveedor.Nombre,
                    concepto = compra.Concepto,
                    costoTotalGravado = compra.Costo_Total_Grabado,
                    iva = compra.Iva,
                    total = compra.Total,
                    productos = compra.Compra_Detalles.Select(cd => new
                    {
                        producto = cd.Producto.Descripcion,
                        cantidad = cd.Cantidad,
                        costoUnitario = cd.Costo_Unitario,
                        porcentajeDescuento = cd.Porcentaje_Descuento,
                        montoDescuento = cd.Monto_Descuento,
                        montoImpuesto = cd.Monto_Impuesto,
                        subtotal = cd.Subtotal
                    }).ToList()
                }
            };

            return new JsonResult(detalle);
        }
    }
}