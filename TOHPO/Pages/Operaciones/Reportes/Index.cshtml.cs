using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
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
    .Include(v => v.Detalle_Ventas)
        .ThenInclude(d => d.Producto)
    .Include(v => v.Venta_Metodo_Pagos)
        .ThenInclude(vm => vm.Metodo_Pago)
    .Where(v => v.Fecha >= FechaInicio && v.Fecha <= FechaFin)
    .OrderBy(v => v.Fecha)
    .ToListAsync();

            var htmlContent = GenerarHtmlVentas(ventas);
            return ConvertirHtmlAPdf(htmlContent);
        }


        private async Task<byte[]> GenerarReporteComprasAsync()
        {
            var compras = await _context.Compra
                .Include(c => c.Proveedor)
                .Include(c => c.Compra_Detalles)
                    .ThenInclude(d => d.Producto)
                .Include(c => c.Compra_Metodo_Pagos)
                    .ThenInclude(mp => mp.Metodo_Pago)
                .Where(c => c.Fecha >= FechaInicio && c.Fecha <= FechaFin)
                .OrderBy(c => c.Fecha)
                .ThenBy(c => c.Hora)
                .ToListAsync();

            string htmlContent = GenerarHtmlCompras(compras);
            return ConvertirHtmlAPdf(htmlContent);
        }


        private async Task<byte[]> GenerarReporteInventarioAsync()
        {
            var inventario = await _context.Inventario
                .Include(i => i.Producto)
                .ThenInclude(p => p.Categoria)
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
            // Si no hay ventas, devolvemos algo simple
            if (ventas == null || !ventas.Any())
            {
                return @"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='utf-8'>
            <title>Reporte de Ventas</title>
            <style>
                body { font-family: Arial, sans-serif; margin: 20px; }
                h1 { color: #333; text-align: center; }
                p { font-size: 12px; }
            </style>
        </head>
        <body>
            <h1>Reporte de Ventas</h1>
            <p><strong>Período:</strong> " + FechaInicio.ToString("dd/MM/yyyy") + @" - " + FechaFin.ToString("dd/MM/yyyy") + @"</p>
            <p><strong>Generado:</strong> " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + @"</p>
            <p>No se encontraron ventas en el período seleccionado.</p>
        </body>
        </html>";
            }

            // ---------- RESUMEN GENERAL ----------
            var totalGravado = ventas.Sum(v => v.Costo_Total_Gravado);
            var totalIva = ventas.Sum(v => v.Iva);
            var totalVentas = ventas.Sum(v => v.Total);
            var totalDescuentos = ventas
                .SelectMany(v => v.Detalle_Ventas)
                .Sum(d => d.Monto_Descuento);

            var sb = new StringBuilder();

            sb.Append(@"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='utf-8'>
        <title>Reporte de Ventas</title>
        <style>
            body { font-family: Arial, sans-serif; margin: 20px; }
            h1 { color: #333; text-align: center; }
            h2, h3, h4 { color: #444; margin-top: 25px; }
            p { font-size: 12px; }

            .resumen-table {
                width: 100%;
                border-collapse: collapse;
                margin-top: 15px;
                margin-bottom: 25px;
            }
            .resumen-table td {
                padding: 4px 8px;
                font-size: 12px;
            }
            .texto-izquierda { text-align: left; }
            .texto-derecha { text-align: right; }

            table.detalle-venta {
                width: 100%;
                border-collapse: collapse;
                margin-top: 10px;
                margin-bottom: 25px;
                font-size: 11px;
            }
            table.detalle-venta thead tr {
                border-bottom: 1px solid #999;
            }
            table.detalle-venta tfoot tr {
                border-top: 1px solid #999;
            }
            table.detalle-venta th {
                padding: 6px 4px;
                font-weight: bold;
                text-align: left;
            }
            table.detalle-venta td {
                padding: 4px 4px;
            }

            .venta-info-table {
                width: 100%;
                border-collapse: collapse;
                margin-top: 10px;
                margin-bottom: 5px;
                font-size: 11px;
            }
            .venta-info-table td {
                padding: 3px 4px;
            }

            .separador {
                border-top: 1px solid #ccc;
                margin-top: 15px;
                margin-bottom: 15px;
            }

            /* ------- Estilos para ""gráficas"" tipo barra ------- */
            .chart-section {
                margin-top: 30px;
                page-break-inside: avoid;
            }
            .chart-row {
                display: flex;
                align-items: center;
                margin-bottom: 4px;
                font-size: 11px;
            }
            .chart-label {
                width: 35%;
                text-align: left;
                padding-right: 8px;
                white-space: nowrap;
                overflow: hidden;
                text-overflow: ellipsis;
            }
            .chart-bar-container {
                flex: 1;
                background-color: #f2f2f2;
                height: 14px;
                border-radius: 7px;
                overflow: hidden;
                margin-right: 8px;
            }
            .chart-bar {
                height: 100%;
                background-color: #4a90e2;
            }
            .chart-value {
                width: 80px;
                text-align: right;
            }
        </style>
    </head>
    <body>
        <h1>Reporte de Ventas</h1>
        <p><strong>Período:</strong> " + FechaInicio.ToString("dd/MM/yyyy") + @" - " + FechaFin.ToString("dd/MM/yyyy") + @"</p>
        <p><strong>Generado:</strong> " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + @"</p>

        <h2>Resumen General</h2>
        <table class='resumen-table'>
            <tr>
                <td class='texto-izquierda'><strong>Número de ventas:</strong></td>
                <td class='texto-derecha'>" + ventas.Count + @"</td>
            </tr>
            <tr>
                <td class='texto-izquierda'><strong>Total gravado:</strong></td>
                <td class='texto-derecha'>₡" + totalGravado.ToString("N2") + @"</td>
            </tr>
            <tr>
                <td class='texto-izquierda'><strong>Total descuentos:</strong></td>
                <td class='texto-derecha'>₡" + totalDescuentos.ToString("N2") + @"</td>
            </tr>
            <tr>
                <td class='texto-izquierda'><strong>Total IVA:</strong></td>
                <td class='texto-derecha'>₡" + totalIva.ToString("N2") + @"</td>
            </tr>
            <tr>
                <td class='texto-izquierda'><strong>Total vendido:</strong></td>
                <td class='texto-derecha'>₡" + totalVentas.ToString("N2") + @"</td>
            </tr>
        </table>
    ");

            // ---------- DETALLE POR VENTA (CON CONTADOR SECUENCIAL) ----------
            int contadorVentas = 1;

            foreach (var venta in ventas.OrderBy(v => v.Fecha).ThenBy(v => v.Hora))
            {
                var clienteNombre = venta.Cliente != null
                    ? $"{venta.Cliente.Nombre} {venta.Cliente.Primer_Apellido} {venta.Cliente.Segundo_Apellido}"
                    : "Sin cliente";

                var metodosPago = (venta.Venta_Metodo_Pagos != null && venta.Venta_Metodo_Pagos.Any())
                    ? string.Join(", ", venta.Venta_Metodo_Pagos.Select(mp =>
                        $"{(mp.Metodo_Pago != null ? mp.Metodo_Pago.Descripcion : "N/A")}: ₡{mp.Monto.ToString("N2")}"))
                    : "N/A";

                var subtotalDetalles = venta.Detalle_Ventas.Sum(d => d.Subtotal);
                var descuentoDetalles = venta.Detalle_Ventas.Sum(d => d.Monto_Descuento);
                var ivaDetalles = venta.Detalle_Ventas.Sum(d => d.Monto_Impuesto);
                var totalVenta = venta.Total;

                sb.Append(@"
        <h3>Venta #" + contadorVentas + @"</h3>

        <table class='venta-info-table'>
            <tr>
                <td><strong>Fecha:</strong> " + venta.Fecha.ToString("dd/MM/yyyy") + @"</td>
                <td><strong>Hora:</strong> " + venta.Hora.ToString("HH:mm") + @"</td>
                <td><strong>Cliente:</strong> " + clienteNombre + @"</td>
            </tr>
            <tr>
                <td><strong>Cédula:</strong> " + (venta.Cliente != null ? venta.Cliente.Cedula : "N/A") + @"</td>
                <td colspan='2'><strong>Método(s) de pago:</strong> " + metodosPago + @"</td>
            </tr>
            <tr>
                <td colspan='3'><strong>Concepto:</strong> " + (string.IsNullOrWhiteSpace(venta.Concepto) ? "-" : venta.Concepto) + @"</td>
            </tr>
        </table>

        <table class='detalle-venta'>
            <thead>
                <tr>
                    <th class='texto-izquierda'>Producto</th>
                    <th class='texto-derecha'>Cantidad</th>
                    <th class='texto-derecha'>Precio Unitario</th>
                    <th class='texto-derecha'>Descuento</th>
                    <th class='texto-derecha'>IVA</th>
                    <th class='texto-derecha'>Subtotal</th>
                </tr>
            </thead>
            <tbody>");

                foreach (var det in venta.Detalle_Ventas)
                {
                    var nombreProducto = det.Producto != null
                        ? det.Producto.Descripcion
                        : (det.Codigo_Producto ?? "N/A");

                    sb.Append(@"
                <tr>
                    <td class='texto-izquierda'>" + nombreProducto + @"</td>
                    <td class='texto-derecha'>" + det.Cantidad + @"</td>
                    <td class='texto-derecha'>₡" + det.Precio_Unitario.ToString("N2") + @"</td>
                    <td class='texto-derecha'>₡" + det.Monto_Descuento.ToString("N2") + @"</td>
                    <td class='texto-derecha'>₡" + det.Monto_Impuesto.ToString("N2") + @"</td>
                    <td class='texto-derecha'>₡" + det.Subtotal.ToString("N2") + @"</td>
                </tr>");
                }

                sb.Append(@"
            </tbody>
            <tfoot>
                <tr>
                    <td class='texto-izquierda'><strong>Totales venta #" + contadorVentas + @"</strong></td>
                    <td></td>
                    <td class='texto-derecha'><strong>Subtotal:</strong></td>
                    <td class='texto-derecha'>₡" + subtotalDetalles.ToString("N2") + @"</td>
                    <td class='texto-derecha'><strong>IVA:</strong></td>
                    <td class='texto-derecha'><strong>₡" + totalVenta.ToString("N2") + @"</strong></td>
                </tr>
            </tfoot>
        </table>

        <div class='separador'></div>
        ");

                contadorVentas++;
            }

            // ---------- "GRÁFICAS" TIPO BARRA ----------

            // 1) Productos más vendidos (por cantidad)
            var productosCantidad = ventas
                .SelectMany(v => v.Detalle_Ventas)
                .GroupBy(d => d.Producto != null
                                ? d.Producto.Descripcion
                                : (d.Codigo_Producto ?? "N/A"))
                .Select(g => new { Nombre = g.Key, Cantidad = g.Sum(d => d.Cantidad) })
                .OrderByDescending(x => x.Cantidad)
                .Take(15)
                .ToList();

            int maxCantProd = productosCantidad.Any() ? productosCantidad.Max(x => x.Cantidad) : 1;

            // 2) Ventas por cliente (por total de dinero)
            var ventasPorCliente = ventas
                .GroupBy(v => v.Cliente != null
                                ? (v.Cliente.Nombre + " " + v.Cliente.Primer_Apellido + " " + v.Cliente.Segundo_Apellido)
                                : "Sin cliente")
                .Select(g => new { Cliente = g.Key, Total = g.Sum(v => v.Total) })
                .OrderByDescending(x => x.Total)
                .Take(15)
                .ToList();

            decimal maxTotalCliente = ventasPorCliente.Any() ? ventasPorCliente.Max(x => x.Total) : 1m;

            // 3) Ventas por fecha
            var ventasPorFecha = ventas
                .GroupBy(v => v.Fecha.Date)
                .Select(g => new { Fecha = g.Key, Total = g.Sum(v => v.Total) })
                .OrderBy(x => x.Fecha)
                .ToList();

            decimal maxTotalFecha = ventasPorFecha.Any() ? ventasPorFecha.Max(x => x.Total) : 1m;

            sb.Append(@"
        <h2>Resumen Gráfico</h2>

        <div class='chart-section'>
            <h4>Productos más vendidos (por cantidad)</h4>");

            foreach (var p in productosCantidad)
            {
                var width = maxCantProd > 0 ? (int)(p.Cantidad * 100.0 / maxCantProd) : 0;

                sb.Append(@"
            <div class='chart-row'>
                <div class='chart-label'>" + p.Nombre + @"</div>
                <div class='chart-bar-container'>
                    <div class='chart-bar' style='width:" + width + @"%;'></div>
                </div>
                <div class='chart-value'>" + p.Cantidad + @"</div>
            </div>");
            }

            sb.Append(@"
        </div>

        <div class='chart-section'>
            <h4>Ventas por cliente (₡)</h4>");

            foreach (var c in ventasPorCliente)
            {
                var width = maxTotalCliente > 0 ? (int)(c.Total * 100m / maxTotalCliente) : 0;

                sb.Append(@"
            <div class='chart-row'>
                <div class='chart-label'>" + c.Cliente + @"</div>
                <div class='chart-bar-container'>
                    <div class='chart-bar' style='width:" + width + @"%;'></div>
                </div>
                <div class='chart-value'>₡" + c.Total.ToString("N2") + @"</div>
            </div>");
            }

            sb.Append(@"
        </div>

        <div class='chart-section'>
            <h4>Ventas por fecha (₡)</h4>");

            foreach (var f in ventasPorFecha)
            {
                var width = maxTotalFecha > 0 ? (int)(f.Total * 100m / maxTotalFecha) : 0;

                sb.Append(@"
            <div class='chart-row'>
                <div class='chart-label'>" + f.Fecha.ToString("dd/MM/yyyy") + @"</div>
                <div class='chart-bar-container'>
                    <div class='chart-bar' style='width:" + width + @"%;'></div>
                </div>
                <div class='chart-value'>₡" + f.Total.ToString("N2") + @"</div>
            </div>");
            }

            sb.Append(@"
        </div>

    </body>
    </html>");

            return sb.ToString();
        }




        private string GenerarHtmlCompras(List<Compra> compras)
        {
            var culture = new CultureInfo("es-CR");

            // Flateamos los detalles
            var todosDetalles = compras.SelectMany(c => c.Compra_Detalles).ToList();

            // Totales generales
            decimal totalCompras = compras.Sum(c => c.Total);
            decimal totalGravado = compras.Sum(c => c.Costo_Total_Grabado);
            decimal totalIva = compras.Sum(c => c.Iva);
            decimal totalDescuentos = todosDetalles.Sum(d => d.Monto_Descuento);
            decimal totalImpuestoDetalles = todosDetalles.Sum(d => d.Monto_Impuesto);
            decimal totalSubtotalDetalles = todosDetalles.Sum(d => d.Subtotal);

            // Para “gráficos” de barra: productos más comprados
            var comprasPorProducto = todosDetalles
                .GroupBy(d => d.Producto?.Descripcion ?? "Producto sin nombre")
                .Select(g => new
                {
                    Producto = g.Key,
                    Cantidad = g.Sum(d => d.Cantidad)
                })
                .OrderByDescending(x => x.Cantidad)
                .Take(10)
                .ToList();

            int maxCantidadProd = comprasPorProducto.Any() ? comprasPorProducto.Max(x => x.Cantidad) : 1;

            // Compras por proveedor
            var comprasPorProveedor = compras
                .GroupBy(c => c.Proveedor?.Nombre ?? "Proveedor sin nombre")
                .Select(g => new
                {
                    Proveedor = g.Key,
                    Total = g.Sum(c => c.Total)
                })
                .OrderByDescending(x => x.Total)
                .Take(10)
                .ToList();

            decimal maxTotalProveedor = comprasPorProveedor.Any() ? comprasPorProveedor.Max(x => x.Total) : 1m;

            // Compras por día
            var comprasPorDia = compras
                .GroupBy(c => c.Fecha.Date)
                .Select(g => new
                {
                    Fecha = g.Key,
                    Total = g.Sum(c => c.Total)
                })
                .OrderBy(x => x.Fecha)
                .ToList();

            decimal maxTotalDia = comprasPorDia.Any() ? comprasPorDia.Max(x => x.Total) : 1m;

            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Reporte de Compras</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            margin: 20px;
            font-size: 11px;
        }}
        h1, h2, h3 {{
            color: #333;
        }}
        h1 {{
            text-align: center;
            margin-bottom: 5px;
        }}
        .subtitulo {{
            text-align: center;
            color: #666;
            font-size: 10px;
            margin-bottom: 20px;
        }}
        table {{
            width: 100%;
            border-collapse: collapse;
            margin-top: 10px;
            margin-bottom: 20px;
        }}
        th, td {{
            padding: 6px 4px;
            font-size: 10px;
        }}
        th {{
            border-bottom: 1px solid #000;
            text-align: left;
            background-color: #f5f5f5;
        }}
        tr:last-child td {{
            border-bottom: 1px solid #000;
        }}
        .text-right {{
            text-align: right;
        }}
        .text-left {{
            text-align: left;
        }}
        .text-center {{
            text-align: center;
        }}
        .resumen-table td {{
            border: none;
            padding: 2px 4px;
        }}
        .resumen-label {{
            font-weight: bold;
            width: 220px;
        }}
        .section-title {{
            margin-top: 25px;
            border-bottom: 1px solid #333;
            padding-bottom: 3px;
        }}
        .compra-header {{
            background-color: #e9f2ff;
            padding: 8px;
            border-radius: 4px;
            margin-top: 10px;
            margin-bottom: 5px;
        }}
        .compra-header span {{
            display: inline-block;
            margin-right: 10px;
            font-size: 10px;
        }}
        .totales-venta {{
            font-weight: bold;
            background-color: #f9f9f9;
        }}
        .badge {{
            display: inline-block;
            padding: 2px 6px;
            border-radius: 4px;
            font-size: 9px;
        }}
        .badge-proveedor {{
            background-color: #007bff;
            color: white;
        }}
        .badge-total {{
            background-color: #28a745;
            color: white;
        }}
        .chart-table td {{
            border: none;
            padding: 3px 4px;
        }}
        .bar-container {{
            width: 100%;
            background-color: #f0f0f0;
            height: 10px;
        }}
        .bar-fill {{
            height: 10px;
            background-color: #007bff;
        }}
        .small-text {{
            font-size: 9px;
            color: #555;
        }}
    </style>
</head>
<body>
    <h1>Reporte de Compras</h1>
    <div class='subtitulo'>
        <div><strong>Período:</strong> {FechaInicio:dd/MM/yyyy} - {FechaFin:dd/MM/yyyy}</div>
        <div><strong>Generado:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</div>
    </div>

    <!-- RESUMEN GENERAL -->
    <h3 class='section-title'>Resumen General</h3>
    <table class='resumen-table'>
        <tr>
            <td class='resumen-label'>Número de compras:</td>
            <td class='text-right'>{compras.Count}</td>
        </tr>
        <tr>
            <td class='resumen-label'>Total costo gravado:</td>
            <td class='text-right'>₡{totalGravado.ToString("N2", culture)}</td>
        </tr>
        <tr>
            <td class='resumen-label'>Total IVA (cabecera):</td>
            <td class='text-right'>₡{totalIva.ToString("N2", culture)}</td>
        </tr>
        <tr>
            <td class='resumen-label'>Total descuentos (detalle):</td>
            <td class='text-right'>₡{totalDescuentos.ToString("N2", culture)}</td>
        </tr>
        <tr>
            <td class='resumen-label'>Total impuesto (detalle):</td>
            <td class='text-right'>₡{totalImpuestoDetalles.ToString("N2", culture)}</td>
        </tr>
        <tr>
            <td class='resumen-label'>Subtotal acumulado (detalle):</td>
            <td class='text-right'>₡{totalSubtotalDetalles.ToString("N2", culture)}</td>
        </tr>
        <tr>
            <td class='resumen-label'>Total general comprado:</td>
            <td class='text-right'>₡{totalCompras.ToString("N2", culture)}</td>
        </tr>
    </table>

    <!-- DETALLE POR COMPRA -->
    <h3 class='section-title'>Detalle por Compra</h3>
";

            int consecutivo = 1;

            foreach (var compra in compras)
            {
                var detalles = compra.Compra_Detalles.ToList();
                var metodosPago = compra.Compra_Metodo_Pagos.ToList();

                decimal totalCantidad = detalles.Sum(d => d.Cantidad);
                decimal totalDescCompra = detalles.Sum(d => d.Monto_Descuento);
                decimal totalIvaCompra = detalles.Sum(d => d.Monto_Impuesto);
                decimal totalSubCompra = detalles.Sum(d => d.Subtotal);

                string proveedor = compra.Proveedor?.Nombre ?? "Proveedor no registrado";
                string concepto = string.IsNullOrWhiteSpace(compra.Concepto) ? "Sin concepto" : compra.Concepto;

                html += $@"
    <!-- ENCABEZADO COMPRA -->
    <div class='compra-header'>
        <span><strong>Compra #{consecutivo}</strong> (ID interno: {compra.Id})</span>
        <span><strong>Fecha:</strong> {compra.Fecha:dd/MM/yyyy}</span>
        <span><strong>Hora:</strong> {compra.Hora:HH:mm}</span><br/>
        <span class='badge badge-proveedor'><strong>Proveedor:</strong> {proveedor}</span><br/>
        <span><strong>Concepto:</strong> {concepto}</span><br/>
        <span class='badge badge-total'><strong>Total compra:</strong> ₡{compra.Total.ToString("N2", culture)}</span>
    </div>

    <!-- TABLA DETALLE -->
    <table>
        <thead>
            <tr>
                <th class='text-left'>Producto</th>
                <th class='text-right'>Cantidad</th>
                <th class='text-right'>Costo unitario</th>
                <th class='text-right'>Descuento</th>
                <th class='text-right'>IVA</th>
                <th class='text-right'>Subtotal</th>
            </tr>
        </thead>
        <tbody>";

                foreach (var det in detalles)
                {
                    string nombreProd = det.Producto?.Descripcion ?? det.Codigo_Producto ?? "Producto no registrado";
                    html += $@"
            <tr>
                <td class='text-left'>{nombreProd}</td>
                <td class='text-right'>{det.Cantidad}</td>
                <td class='text-right'>₡{det.Costo_Unitario.ToString("N2", culture)}</td>
                <td class='text-right'>₡{det.Monto_Descuento.ToString("N2", culture)}</td>
                <td class='text-right'>₡{det.Monto_Impuesto.ToString("N2", culture)}</td>
                <td class='text-right'>₡{det.Subtotal.ToString("N2", culture)}</td>
            </tr>";
                }

                html += $@"
            <tr class='totales-venta'>
                <td class='text-left'><strong>Totales de la compra</strong></td>
                <td class='text-right'><strong>{totalCantidad}</strong></td>
                <td class='text-right'>-</td>
                <td class='text-right'><strong>₡{totalDescCompra.ToString("N2", culture)}</strong></td>
                <td class='text-right'><strong>₡{totalIvaCompra.ToString("N2", culture)}</strong></td>
                <td class='text-right'><strong>₡{totalSubCompra.ToString("N2", culture)}</strong></td>
            </tr>
        </tbody>
    </table>
";

                if (metodosPago.Any())
                {
                    html += @"
    <table class='small-text'>
        <thead>
            <tr>
                <th class='text-left'>Método de pago</th>
                <th class='text-right'>Monto</th>
            </tr>
        </thead>
        <tbody>";

                    foreach (var mp in metodosPago)
                    {
                        string metodo = mp.Metodo_Pago?.Descripcion ?? "Método sin nombre";
                        html += $@"
            <tr>
                <td class='text-left'>{metodo}</td>
                <td class='text-right'>₡{mp.Monto.ToString("N2", culture)}</td>
            </tr>";
                    }

                    html += @"
        </tbody>
    </table>";
                }

                html += "<br/>";
                consecutivo++;
            }

            // “Gráficos” de barras al final
            html += @"
    <h3 class='section-title'>Análisis Visual de Compras</h3>

    <!-- Productos más comprados -->
    <h4>1. Productos más comprados (por cantidad)</h4>
    <table class='chart-table'>
        <tbody>";
            foreach (var item in comprasPorProducto)
            {
                var width = maxCantidadProd > 0 ? (int)(item.Cantidad * 100.0 / maxCantidadProd) : 0;
                html += $@"
            <tr>
                <td class='text-left small-text'>{item.Producto}</td>
                <td>
                    <div class='bar-container'>
                        <div class='bar-fill' style='width:{width}%;'></div>
                    </div>
                </td>
                <td class='text-right small-text'>{item.Cantidad}</td>
            </tr>";
            }
            html += @"
        </tbody>
    </table>

    <!-- Compras por proveedor -->
    <h4>2. Compras por proveedor (monto total)</h4>
    <table class='chart-table'>
        <tbody>";
            foreach (var item in comprasPorProveedor)
            {
                var width = maxTotalProveedor > 0 ? (int)(item.Total * 100.0m / maxTotalProveedor) : 0;
                html += $@"
            <tr>
                <td class='text-left small-text'>{item.Proveedor}</td>
                <td>
                    <div class='bar-container'>
                        <div class='bar-fill' style='width:{width}%;'></div>
                    </div>
                </td>
                <td class='text-right small-text'>₡{item.Total.ToString("N2", culture)}</td>
            </tr>";
            }
            html += @"
        </tbody>
    </table>

    <!-- Compras por día -->
    <h4>3. Compras por día</h4>
    <table class='chart-table'>
        <tbody>";
            foreach (var item in comprasPorDia)
            {
                var width = maxTotalDia > 0 ? (int)(item.Total * 100.0m / maxTotalDia) : 0;
                html += $@"
            <tr>
                <td class='text-left small-text'>{item.Fecha:dd/MM/yyyy}</td>
                <td>
                    <div class='bar-container'>
                        <div class='bar-fill' style='width:{width}%;'></div>
                    </div>
                </td>
                <td class='text-right small-text'>₡{item.Total.ToString("N2", culture)}</td>
            </tr>";
            }
            html += @"
        </tbody>
    </table>

</body>
</html>";

            return html;
        }

        private string FormatearColones(decimal valor)
        {
            return valor.ToString("N2", new CultureInfo("es-CR"));
        }

        private string GenerarHtmlInventario(List<TOHPO.Models.Inventario> inventario)
        {
            var totalProductos = inventario.Count;
            var totalExistencias = inventario.Sum(i => i.Existencia);
            var totalReservado = inventario.Sum(i => i.Reservado);
            var productosStockBajo = inventario.Count(i => i.Disponible <= 10);
            var valorTotalInventario = inventario.Sum(i => i.Existencia * i.Precio_Venta);

            var gruposPorCategoria = inventario
                .GroupBy(i => i.Producto?.Categoria?.Descripcion ?? "Sin categoría")
                .OrderBy(g => g.Key)
                .ToList();

            var html = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='utf-8'>
        <title>Reporte de Inventario</title>
        <style>
            body {{ font-family: Arial, sans-serif; margin: 20px; }}
            h1 {{ color: #333; text-align: center; }}
            h2 {{ color: #444; margin-top: 25px; }}
            h3 {{ color: #555; margin-top: 20px; }}

            table {{ width: 100%; border-collapse: collapse; margin-top: 10px; }}
            th, td {{ padding: 6px 8px; font-size: 12px; }}
            th {{
                border-bottom: 1px solid #000;
                text-align: left;
                background-color: #f2f2f2;
            }}
            tr.ultima-fila td {{
                border-bottom: 1px solid #000;
            }}

            .texto-izquierda {{ text-align: left; }}
            .numero-derecha {{ text-align: right; white-space: nowrap; }}

            .low-stock {{ background-color: #ffefef; }}
            .subtotal-row td {{
                font-weight: bold;
                border-top: 1px solid #000;
            }}

            .resumen-table {{
                width: 50%;
                margin-top: 15px;
                margin-bottom: 25px;
            }}
            .resumen-table td {{
                padding: 4px 6px;
                font-size: 12px;
            }}
            .resumen-item {{
                text-align: left;
            }}
        </style>
    </head>
    <body>
        <h1>Reporte de Inventario</h1>
        <p><strong>Generado:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>

        <h2>Resumen general</h2>
        <table class='resumen-table'>
            <tr><td class='resumen-item'><strong>Total de productos:</strong> {totalProductos}</td></tr>
            <tr><td class='resumen-item'><strong>Total de existencias:</strong> {totalExistencias}</td></tr>
            <tr><td class='resumen-item'><strong>Total reservado:</strong> {totalReservado}</td></tr>
            <tr><td class='resumen-item'><strong>Productos con stock bajo (≤ 10):</strong> {productosStockBajo}</td></tr>
            <tr>
                <td class='resumen-item'>
                    <strong>Valor total del inventario:</strong> ₡{FormatearColones(valorTotalInventario)}
                </td>
            </tr>
        </table>
    ";

            foreach (var grupo in gruposPorCategoria)
            {
                var productosCategoria = grupo.ToList();

                var totalExistenciaCat = productosCategoria.Sum(i => i.Existencia);
                var totalReservadoCat = productosCategoria.Sum(i => i.Reservado);
                var totalDisponibleCat = productosCategoria.Sum(i => i.Disponible);
                var totalValorCategoria = productosCategoria.Sum(i => i.Existencia * i.Precio_Venta);

                html += $@"
        <h3>Categoría: {grupo.Key}</h3>
        <table>
            <thead>
                <tr>
                    <th class='texto-izquierda'>Producto</th>
                    <th class='texto-izquierda'>Presentación</th>
                    <th class='numero-derecha'>Existencia</th>
                    <th class='numero-derecha'>Reservado</th>
                    <th class='numero-derecha'>Disponible</th>
                    <th class='numero-derecha'>Precio Venta</th>
                    <th class='numero-derecha'>Valor Total</th>
                    <th class='texto-izquierda'>Estado</th>
                </tr>
            </thead>
            <tbody>";

                foreach (var item in productosCategoria)
                {
                    var esStockBajo = item.Disponible <= 10;
                    var claseRow = esStockBajo ? "low-stock" : "";
                    var estado = esStockBajo ? "Stock Bajo" : "OK";

                    var presentacionTexto = $"{item.Producto?.Presentacion?.Cantidad ?? 0} {item.Producto?.Presentacion?.Unidad_Medida}";
                    var valorTotal = item.Existencia * item.Precio_Venta;

                    html += $@"
                <tr class='{claseRow}'>
                    <td class='texto-izquierda'>{item.Producto?.Descripcion ?? "N/A"}</td>
                    <td class='texto-izquierda'>{presentacionTexto}</td>
                    <td class='numero-derecha'>{item.Existencia}</td>
                    <td class='numero-derecha'>{item.Reservado}</td>
                    <td class='numero-derecha'>{item.Disponible}</td>
                    <td class='numero-derecha'>₡{FormatearColones(item.Precio_Venta)}</td>
                    <td class='numero-derecha'>₡{FormatearColones(valorTotal)}</td>
                    <td class='texto-izquierda'>{estado}</td>
                </tr>";
                }

                html += $@"
                <tr class='subtotal-row ultima-fila'>
                    <td colspan='2' class='texto-izquierda'>Subtotal categoría</td>
                    <td class='numero-derecha'>{totalExistenciaCat}</td>
                    <td class='numero-derecha'>{totalReservadoCat}</td>
                    <td class='numero-derecha'>{totalDisponibleCat}</td>
                    <td></td>
                    <td class='numero-derecha'>₡{FormatearColones(totalValorCategoria)}</td>
                    <td></td>
                </tr>
            </tbody>
        </table>";
            }

            html += "</body></html>";

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