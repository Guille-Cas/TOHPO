using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TOHPO.Data;
using TOHPO.Models;

namespace TOHPO.Pages.Operaciones.Ventas
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Venta> Ventas { get; set; } = default!;

        [BindProperty]
        public DateTime? FechaInicio { get; set; }

        [BindProperty]
        public DateTime? FechaFin { get; set; }

        [BindProperty]
        public string? BuscarCliente { get; set; }

        public async Task OnGetAsync()
        {
            await CargarVentas();
        }

        public async Task<IActionResult> OnPostFiltrarAsync()
        {
            await CargarVentas();
            return Page();
        }

        private async Task CargarVentas()
        {
            if (_context.Venta != null)
            {
                var query = _context.Venta
                    .Include(v => v.Cliente)
                    .Include(v => v.Agente_Ventas)
                    .Include(v => v.Detalle_Ventas)
                        .ThenInclude(dv => dv.Producto)
                    .AsQueryable();

                // Filtros
                if (FechaInicio.HasValue)
                {
                    query = query.Where(v => v.Fecha >= FechaInicio.Value);
                }

                if (FechaFin.HasValue)
                {
                    query = query.Where(v => v.Fecha <= FechaFin.Value);
                }

                if (!string.IsNullOrEmpty(BuscarCliente))
                {
                    query = query.Where(v => v.Cliente.Nombre.Contains(BuscarCliente) ||
                                           v.Cliente.Primer_Apellido.Contains(BuscarCliente) ||
                                           v.Cliente.Segundo_Apellido.Contains(BuscarCliente));
                }

                Ventas = await query
                    .OrderByDescending(v => v.Fecha)
                    .ThenByDescending(v => v.Hora)
                    .ToListAsync();
            }
        }

        public async Task<IActionResult> OnGetEliminarAsync(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "ID de venta no válido";
                return RedirectToPage();
            }

            var venta = await _context.Venta
                .Include(v => v.Detalle_Ventas)
                .Include(v => v.Venta_Metodo_Pagos)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
            {
                TempData["ErrorMessage"] = "Venta no encontrada";
                return RedirectToPage();
            }

            try
            {
                // Eliminar detalles de venta
                if (venta.Detalle_Ventas.Any())
                {
                    _context.Detalle_Venta.RemoveRange(venta.Detalle_Ventas);
                }

                // Eliminar métodos de pago
                if (venta.Venta_Metodo_Pagos.Any())
                {
                    _context.Venta_Metodo_Pago.RemoveRange(venta.Venta_Metodo_Pagos);
                }

                // Eliminar la venta
                _context.Venta.Remove(venta);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Venta eliminada exitosamente";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar la venta: " + ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<JsonResult> OnGetDetalleVentaAsync(int id)
        {
            var venta = await _context.Venta
                .Include(v => v.Cliente)
                .Include(v => v.Agente_Ventas)
                .Include(v => v.Detalle_Ventas)
                    .ThenInclude(dv => dv.Producto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
            {
                return new JsonResult(new { success = false, message = "Venta no encontrada" });
            }

            var detalle = new
            {
                success = true,
                venta = new
                {
                    id = venta.Id,
                    fecha = venta.Fecha.ToString("dd/MM/yyyy"),
                    hora = venta.Hora.ToString("HH:mm"),
                    cliente = $"{venta.Cliente.Nombre} {venta.Cliente.Primer_Apellido}",
                    agente = venta.Agente_Ventas.Nombre,
                    concepto = venta.Concepto,
                    costoTotalGravado = venta.Costo_Total_Gravado,
                    iva = venta.Iva,
                    total = venta.Total,
                    productos = venta.Detalle_Ventas.Select(dv => new
                    {
                        producto = dv.Producto.Descripcion,
                        cantidad = dv.Cantidad,
                        precioUnitario = dv.Precio_Unitario,
                        porcentajeDescuento = dv.Porcentaje_Descuento,
                        montoDescuento = dv.Monto_Descuento,
                        montoImpuesto = dv.Monto_Impuesto,
                        subtotal = dv.Subtotal
                    }).ToList()
                }
            };

            return new JsonResult(detalle);
        }
    }
}