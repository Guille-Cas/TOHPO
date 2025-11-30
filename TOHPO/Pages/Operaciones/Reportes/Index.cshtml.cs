using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TOHPO.Data;
using TOHPO.Models;

namespace TOHPO.Pages.Operaciones.Reportes
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public DateTime FechaInicio { get; set; } = DateTime.Now.AddDays(-30);

        [BindProperty]
        public DateTime FechaFin { get; set; } = DateTime.Now;

        [BindProperty]
        public string TipoReporte { get; set; } = "ventas";

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostGenerarReporteAsync()
        {
            try
            {
                byte[] pdfBytes = null;
                string fileName = "";

                switch (TipoReporte.ToLower())
                {
                    case "ventas":
                        pdfBytes = await GenerarReporteVentasAsync();
                        fileName = $"Reporte_Ventas_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                        break;
                    case "compras":
                        pdfBytes = await GenerarReporteComprasAsync();
                        fileName = $"Reporte_Compras_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                        break;
                    case "inventario":
                        pdfBytes = await GenerarReporteInventarioAsync();
                        fileName = $"Reporte_Inventario_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                        break;
                    case "pedidos":
                        pdfBytes = await GenerarReportePedidosAsync();
                        fileName = $"Reporte_Pedidos_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                        break;
                    default:
                        TempData["Error"] = "Tipo de reporte no válido";
                        return RedirectToPage();
                }

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al generar el reporte: {ex.Message}";
                return RedirectToPage();
            }
        }

        private async Task<byte[]> GenerarReporteVentasAsync()
        {
            var ventas = await _context.Venta
                .Include(v => v.Cliente)
                .Where(v => v.Fecha >= FechaInicio && v.Fecha <= FechaFin)
                .OrderBy(v => v.Fecha)
                .ToListAsync();

            string htmlContent = GenerarHtmlVentas(ventas);
            return ConvertirHtmlAPdf(htmlContent);
        }

        private async Task<byte[]> GenerarReporteComprasAsync()
        {
            var compras = await _context.Compra
                .Include(c => c.Proveedor)
                .Where(c => c.Fecha >= FechaInicio && c.Fecha <= FechaFin)
                .OrderBy(c => c.Fecha)
                .ToListAsync();

            string htmlContent = GenerarHtmlCompras(compras);
            return ConvertirHtmlAPdf(htmlContent);
        }

        private async Task<byte[]> GenerarReporteInventarioAsync()
        {
            var inventario = await _context.Inventario
                .Include(i => i.Producto)
                .ThenInclude(p => p.Presentacion)
                .ToListAsync();

            string htmlContent = GenerarHtmlInventario(inventario);
            return ConvertirHtmlAPdf(htmlContent);
        }

        private async Task<byte[]> GenerarReportePedidosAsync()
        {
            var pedidos = await _context.Pedido
                .Include(p => p.Cliente)
                .Where(p => p.Fecha_Creacion >= FechaInicio && p.Fecha_Creacion <= FechaFin)
                .OrderBy(p => p.Fecha_Creacion)
                .ToListAsync();

            string htmlContent = GenerarHtmlPedidos(pedidos);
            return ConvertirHtmlAPdf(htmlContent);
        }

        private byte[] ConvertirHtmlAPdf(string htmlContent)
        {
            var converter = new SelectPdf.HtmlToPdf();
            
            // Configuración del PDF
            converter.Options.PdfPageSize = SelectPdf.PdfPageSize.A4;
            converter.Options.PdfPageOrientation = SelectPdf.PdfPageOrientation.Portrait;
            converter.Options.MarginTop = 20;
            converter.Options.MarginBottom = 20;
            converter.Options.MarginLeft = 20;
            converter.Options.MarginRight = 20;

            var doc = converter.ConvertHtmlString(htmlContent);
            byte[] pdf = doc.Save();
            doc.Close();

            return pdf;
        }

        private string GenerarHtmlVentas(List<Venta> ventas)
        {
            var html = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <title>Reporte de Ventas</title>
                <style>
                    body {{ font-family: Arial, sans-serif; margin: 20px; }}
                    h1 {{ color: #333; text-align: center; }}
                    table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
                    th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
                    th {{ background-color: #f2f2f2; }}
                    .total {{ font-weight: bold; }}
                </style>
            </head>
            <body>
                <h1>Reporte de Ventas</h1>
                <p><strong>Período:</strong> {FechaInicio:dd/MM/yyyy} - {FechaFin:dd/MM/yyyy}</p>
                <p><strong>Generado:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                
                <table>
                    <thead>
                        <tr>
                            <th>Fecha</th>
                            <th>Cliente</th>
                            <th>Total</th>
                            <th>Estado</th>
                        </tr>
                    </thead>
                    <tbody>";

            foreach (var venta in ventas)
            {
                html += $@"
                        <tr>
                            <td>{venta.Fecha:dd/MM/yyyy}</td>
                            <td>{venta.Cliente?.Nombre ?? "N/A"}</td>
                            <td>₡{venta.Total:F2}</td>
                        </tr>";
            }

            var totalVentas = ventas.Sum(v => v.Total);
            html += $@"
                    </tbody>
                </table>
                <br>
                <p class='total'>Total de Ventas: ₡{totalVentas:F2}</p>
                <p class='total'>Número de Ventas: {ventas.Count}</p>
            </body>
            </html>";

            return html;
        }

        private string GenerarHtmlCompras(List<Compra> compras)
        {
            var html = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <title>Reporte de Compras</title>
                <style>
                    body {{ font-family: Arial, sans-serif; margin: 20px; }}
                    h1 {{ color: #333; text-align: center; }}
                    table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
                    th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
                    th {{ background-color: #f2f2f2; }}
                    .total {{ font-weight: bold; }}
                </style>
            </head>
            <body>
                <h1>Reporte de Compras</h1>
                <p><strong>Período:</strong> {FechaInicio:dd/MM/yyyy} - {FechaFin:dd/MM/yyyy}</p>
                <p><strong>Generado:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                
                <table>
                    <thead>
                        <tr>
                            <th>Fecha</th>
                            <th>Proveedor</th>
                            <th>Total</th>
                            <th>Estado</th>
                        </tr>
                    </thead>
                    <tbody>";

            foreach (var compra in compras)
            {
                html += $@"
                        <tr>
                            <td>{compra.Fecha:dd/MM/yyyy}</td>
                            <td>{compra.Proveedor?.Nombre ?? "N/A"}</td>
                            <td>₡{compra.Total:F2}</td>
                        </tr>";
            }

            var totalCompras = compras.Sum(c => c.Total);
            html += $@"
                    </tbody>
                </table>
                <br>
                <p class='total'>Total de Compras: ₡{totalCompras:F2}</p>
                <p class='total'>Número de Compras: {compras.Count}</p>
            </body>
            </html>";

            return html;
        }

        private string GenerarHtmlInventario(List<TOHPO.Models.Inventario> inventario)
        {
            var html = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <title>Reporte de Inventario</title>
                <style>
                    body {{ font-family: Arial, sans-serif; margin: 20px; }}
                    h1 {{ color: #333; text-align: center; }}
                    table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
                    th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
                    th {{ background-color: #f2f2f2; }}
                    .low-stock {{ background-color: #ffcccc; }}
                </style>
            </head>
            <body>
                <h1>Reporte de Inventario</h1>
                <p><strong>Generado:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                
                <table>
                    <thead>
                        <tr>
                            <th>Producto</th>
                            <th>Presentación</th>
                            <th>Existencia</th>
                            <th>Reservado</th>
                            <th>Disponible</th>
                            <th>Precio Venta</th>
                            <th>Estado</th>
                        </tr>
                    </thead>
                    <tbody>";

            foreach (var item in inventario)
            {
                var esStockBajo = item.Disponible <= 10; // Consideramos stock bajo cuando hay 10 o menos disponibles
                var claseRow = esStockBajo ? "low-stock" : "";
                var estado = esStockBajo ? "Stock Bajo" : "OK";

                html += $@"
                        <tr class='{claseRow}'>
                            <td>{item.Producto?.Descripcion ?? "N/A"}</td>
                            <td>{item.Producto?.Presentacion?.Cantidad ?? 0} {item.Producto?.Presentacion?.Unidad_Medida}</td>
                            <td>{item.Existencia}</td>
                            <td>{item.Reservado}</td>
                            <td>{item.Disponible}</td>
                            <td>₡{item.Precio_Venta:F2}</td>
                            <td>{estado}</td>
                        </tr>";
            }

            html += $@"
                    </tbody>
                </table>
                <br>
                <p><strong>Total de Productos:</strong> {inventario.Count}</p>
                <p><strong>Productos con Stock Bajo:</strong> {inventario.Count(i => i.Disponible <= 10)}</p>
                <p><strong>Valor Total Inventario:</strong> ₡{inventario.Sum(i => i.Existencia * i.Precio_Venta):F2}</p>
            </body>
            </html>";

            return html;
        }

        private string GenerarHtmlPedidos(List<Pedido> pedidos)
        {
            var html = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <title>Reporte de Pedidos</title>
                <style>
                    body {{ font-family: Arial, sans-serif; margin: 20px; }}
                    h1 {{ color: #333; text-align: center; }}
                    table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
                    th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
                    th {{ background-color: #f2f2f2; }}
                    .total {{ font-weight: bold; }}
                </style>
            </head>
            <body>
                <h1>Reporte de Pedidos</h1>
                <p><strong>Período:</strong> {FechaInicio:dd/MM/yyyy} - {FechaFin:dd/MM/yyyy}</p>
                <p><strong>Generado:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                
                <table>
                    <thead>
                        <tr>
                            <th>Fecha Creación</th>
                            <th>Fecha Entrega</th>
                            <th>Cliente</th>
                            <th>Total</th>
                            <th>Abono</th>
                            <th>Saldo</th>
                            <th>Estado</th>
                        </tr>
                    </thead>
                    <tbody>";

            foreach (var pedido in pedidos)
            {
                html += $@"
                        <tr>
                            <td>{pedido.Fecha_Creacion:dd/MM/yyyy}</td>
                            <td>{pedido.Fecha_Entrega:dd/MM/yyyy}</td>
                            <td>{pedido.Cliente?.Nombre ?? "N/A"}</td>
                            <td>₡{pedido.Total:F2}</td>
                            <td>₡{pedido.Abono:F2}</td>
                            <td>₡{pedido.Saldo:F2}</td>
                            <td>{(pedido.Estado ? "Activo" : "Inactivo")}</td>
                        </tr>";
            }

            var totalPedidos = pedidos.Sum(p => p.Total);
            var totalAbonos = pedidos.Sum(p => p.Abono);
            var totalSaldos = pedidos.Sum(p => p.Saldo);
            
            html += $@"
                    </tbody>
                </table>
                <br>
                <p class='total'>Total de Pedidos: ₡{totalPedidos:F2}</p>
                <p class='total'>Total de Abonos: ₡{totalAbonos:F2}</p>
                <p class='total'>Total de Saldos Pendientes: ₡{totalSaldos:F2}</p>
                <p class='total'>Número de Pedidos: {pedidos.Count}</p>
            </body>
            </html>";

            return html;
        }
    }
}