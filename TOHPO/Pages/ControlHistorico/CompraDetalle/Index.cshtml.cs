using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TOHPO.Data;
using TOHPO.Models;
using TOHPO.Helpers;

namespace TOHPO.Pages.ControlHistorico.CompraDetalle
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

        public IEnumerable<Compra_Detalle> CompraDetalles { get; set; } = new List<Compra_Detalle>();
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
            
            var excel = ExcelReportHelper.GenerarReporteCompraDetalleExcel(
                CompraDetalles.ToList(), 
                FechaInicio, 
                FechaFin, 
                BuscarProducto);

            var fileName = $"Historico_Compra_Detalle_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public async Task<IActionResult> OnPostExportarPdfAsync()
        {
            await CargarDatos();
            
            var pdf = PdfReportHelper.GenerarReporteCompraDetallePdf(
                CompraDetalles.ToList(), 
                FechaInicio, 
                FechaFin, 
                BuscarProducto);

            var fileName = $"Historico_Compra_Detalle_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return File(pdf, "application/pdf", fileName);
        }

        private async Task CargarDatos()
        {
            var query = _context.Compra_Detalle
                .Include(cd => cd.Compra)
                    .ThenInclude(c => c.Proveedor)
                .Include(cd => cd.Producto)
                .AsQueryable();

            // Aplicar filtros
            if (FechaInicio.HasValue)
            {
                query = query.Where(cd => cd.Compra.Fecha >= FechaInicio.Value);
            }

            if (FechaFin.HasValue)
            {
                query = query.Where(cd => cd.Compra.Fecha <= FechaFin.Value);
            }

            if (!string.IsNullOrWhiteSpace(BuscarProducto))
            {
                query = query.Where(cd => cd.Codigo_Producto.Contains(BuscarProducto) ||
                                        cd.Producto.Descripcion.Contains(BuscarProducto));
            }

            CompraDetalles = await query
                .OrderByDescending(cd => cd.Compra.Fecha)
                .ThenBy(cd => cd.Id)
                .ToListAsync();

            // Calcular totales
            TotalSubtotal = CompraDetalles.Sum(cd => cd.Subtotal);
            TotalImpuestos = CompraDetalles.Sum(cd => cd.Monto_Impuesto);
        }
    }
}