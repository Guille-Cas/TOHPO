using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TOHPO.Data;
using TOHPO.Models;
using TOHPO.Helpers;

namespace TOHPO.Pages.ControlHistorico.DetalleVenta
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public DateTime? FechaInicio { get; set; }

        [BindProperty]
        public DateTime? FechaFin { get; set; }

        [BindProperty]
        public string BuscarProducto { get; set; } = string.Empty;

        public IEnumerable<Detalle_Venta> DetalleVentas { get; set; } = new List<Detalle_Venta>();
        public decimal TotalSubtotal { get; set; }
        public decimal TotalImpuestos { get; set; }

        public async Task OnGetAsync()
        {
            await CargarDatos();
        }

        public async Task<IActionResult> OnPostFiltrarAsync()
        {
            await CargarDatos();
            return Page();
        }

        public async Task<IActionResult> OnPostExportarExcelAsync()
        {
            await CargarDatos();
            
            var excel = ExcelReportHelper.GenerarReporteVentaDetalleExcel(
                DetalleVentas.ToList(), 
                FechaInicio, 
                FechaFin, 
                BuscarProducto);

            var fileName = $"Historico_Venta_Detalle_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public async Task<IActionResult> OnPostExportarPdfAsync()
        {
            await CargarDatos();
            
            var pdf = PdfReportHelper.GenerarReporteVentaDetallePdf(
                DetalleVentas.ToList(), 
                FechaInicio, 
                FechaFin, 
                BuscarProducto);

            var fileName = $"Historico_Venta_Detalle_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return File(pdf, "application/pdf", fileName);
        }

        private async Task CargarDatos()
        {
            var query = _context.Detalle_Venta
                .Include(dv => dv.Venta)
                    .ThenInclude(v => v.Cliente)
                .Include(dv => dv.Producto)
                .AsQueryable();

            // Aplicar filtros
            if (FechaInicio.HasValue)
            {
                query = query.Where(dv => dv.Venta.Fecha >= FechaInicio.Value);
            }

            if (FechaFin.HasValue)
            {
                query = query.Where(dv => dv.Venta.Fecha <= FechaFin.Value);
            }

            if (!string.IsNullOrWhiteSpace(BuscarProducto))
            {
                query = query.Where(dv => dv.Codigo_Producto.Contains(BuscarProducto) ||
                                        dv.Producto.Descripcion.Contains(BuscarProducto));
            }

            DetalleVentas = await query
                .OrderByDescending(dv => dv.Venta.Fecha)
                .ThenBy(dv => dv.Id)
                .ToListAsync();

            // Calcular totales
            TotalSubtotal = DetalleVentas.Sum(dv => dv.Subtotal);
            TotalImpuestos = DetalleVentas.Sum(dv => dv.Monto_Impuesto);
        }
    }
}