using SelectPdf;
using System.Text;
using TOHPO.Models;

namespace TOHPO.Helpers
{
    public static class PdfReportHelper
    {
        public static byte[] GenerarReporteCompraDetallePdf(List<Compra_Detalle> compraDetalles, DateTime? fechaInicio, DateTime? fechaFin, string filtroProducto = "")
        {
            var html = GenerarHtmlCompraDetalle(compraDetalles, fechaInicio, fechaFin, filtroProducto);
            return GenerarPdfDesdeHtml(html);
        }

        public static byte[] GenerarReporteVentaDetallePdf(List<Detalle_Venta> ventaDetalles, DateTime? fechaInicio, DateTime? fechaFin, string filtroProducto = "")
        {
            var html = GenerarHtmlVentaDetalle(ventaDetalles, fechaInicio, fechaFin, filtroProducto);
            return GenerarPdfDesdeHtml(html);
        }

        public static byte[] GenerarReporteMovimientosInventarioPdf(List<Movimiento_Inventario> movimientos, DateTime? fechaInicio, DateTime? fechaFin, string filtroMotivo = "")
        {
            var html = GenerarHtmlMovimientosInventario(movimientos, fechaInicio, fechaFin, filtroMotivo);
            return GenerarPdfDesdeHtml(html);
        }

        // Nuevo método sobrecargado para manejar movimientos con stock histórico calculado
        public static byte[] GenerarReporteMovimientosInventarioConStockPdf(List<(Movimiento_Inventario movimiento, int stockHistorico)> movimientosConStock, DateTime? fechaInicio, DateTime? fechaFin, string filtroMotivo = "")
        {
            var html = GenerarHtmlMovimientosInventarioConStock(movimientosConStock, fechaInicio, fechaFin, filtroMotivo);
            return GenerarPdfDesdeHtml(html);
        }

        private static string GenerarHtmlCompraDetalle(List<Compra_Detalle> compraDetalles, DateTime? fechaInicio, DateTime? fechaFin, string filtroProducto)
        {
            var sb = new StringBuilder();
            
            // Configurar período y filtros
            string periodo = "";
            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                periodo = $"Período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}";
            }
            else if (fechaInicio.HasValue)
            {
                periodo = $"Desde: {fechaInicio:dd/MM/yyyy}";
            }
            else if (fechaFin.HasValue)
            {
                periodo = $"Hasta: {fechaFin:dd/MM/yyyy}";
            }
            else
            {
                periodo = "Todos los registros";
            }

            if (!string.IsNullOrEmpty(filtroProducto))
            {
                periodo += $" | Filtro: {filtroProducto}";
            }

            sb.Append($@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <title>Histórico - Detalles de Compras</title>
                <style>
                    body {{ font-family: Arial, sans-serif; margin: 20px; }}
                    h1 {{ color: #333; text-align: center; margin-bottom: 10px; }}
                    h2 {{ color: #666; text-align: center; font-size: 14px; margin-bottom: 20px; }}
                    table {{ width: 100%; border-collapse: collapse; margin-top: 10px; }}
                    th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; font-size: 10px; }}
                    th {{ background-color: #f2f2f2; font-weight: bold; }}
                    .text-right {{ text-align: right; }}
                    .total-row {{ background-color: #f9f9f9; font-weight: bold; }}
                    .header-info {{ text-align: center; margin-bottom: 15px; font-size: 12px; color: #666; }}
                </style>
            </head>
            <body>
                <h1>HISTÓRICO - DETALLES DE COMPRAS</h1>
                <div class='header-info'>
                    <div>{periodo}</div>
                    <div>Generado: {DateTime.Now:dd/MM/yyyy HH:mm}</div>
                </div>
                
                <table>
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Fecha</th>
                            <th>Proveedor</th>
                            <th>Producto</th>
                            <th>Cantidad</th>
                            <th>Costo Unitario</th>
                            <th>Desc. %</th>
                            <th>Monto Desc.</th>
                            <th>Monto Imp.</th>
                            <th>Subtotal</th>
                        </tr>
                    </thead>
                    <tbody>");

            decimal totalSubtotal = 0;
            decimal totalImpuestos = 0;

            foreach (var detalle in compraDetalles)
            {
                sb.Append($@"
                        <tr>
                            <td>{detalle.Id}</td>
                            <td>{detalle.Compra?.Fecha.ToString("dd/MM/yyyy") ?? ""}</td>
                            <td>{detalle.Compra?.Proveedor?.Nombre ?? "N/A"}</td>
                            <td>{detalle.Producto?.Descripcion} ({detalle.Codigo_Producto})</td>
                            <td class='text-right'>{detalle.Cantidad:N0}</td>
                            <td class='text-right'>₡{detalle.Costo_Unitario:N2}</td>
                            <td class='text-right'>{detalle.Porcentaje_Descuento:N2}%</td>
                            <td class='text-right'>₡{detalle.Monto_Descuento:N2}</td>
                            <td class='text-right'>₡{detalle.Monto_Impuesto:N2}</td>
                            <td class='text-right'>₡{detalle.Subtotal:N2}</td>
                        </tr>");

                totalSubtotal += detalle.Subtotal;
                totalImpuestos += detalle.Monto_Impuesto;
            }

            sb.Append($@"
                    </tbody>
                    <tfoot>
                        <tr class='total-row'>
                            <td colspan='8' class='text-right'>TOTAL IMPUESTOS:</td>
                            <td class='text-right'>₡{totalImpuestos:N2}</td>
                            <td></td>
                        </tr>
                        <tr class='total-row'>
                            <td colspan='9' class='text-right'>TOTAL GENERAL:</td>
                            <td class='text-right'>₡{totalSubtotal:N2}</td>
                        </tr>
                    </tfoot>
                </table>
            </body>
            </html>");

            return sb.ToString();
        }

        private static string GenerarHtmlVentaDetalle(List<Detalle_Venta> ventaDetalles, DateTime? fechaInicio, DateTime? fechaFin, string filtroProducto)
        {
            var sb = new StringBuilder();
            
            // Configurar período y filtros
            string periodo = "";
            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                periodo = $"Período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}";
            }
            else if (fechaInicio.HasValue)
            {
                periodo = $"Desde: {fechaInicio:dd/MM/yyyy}";
            }
            else if (fechaFin.HasValue)
            {
                periodo = $"Hasta: {fechaFin:dd/MM/yyyy}";
            }
            else
            {
                periodo = "Todos los registros";
            }

            if (!string.IsNullOrEmpty(filtroProducto))
            {
                periodo += $" | Filtro: {filtroProducto}";
            }

            sb.Append($@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <title>Histórico - Detalles de Ventas</title>
                <style>
                    body {{ font-family: Arial, sans-serif; margin: 20px; }}
                    h1 {{ color: #333; text-align: center; margin-bottom: 10px; }}
                    h2 {{ color: #666; text-align: center; font-size: 14px; margin-bottom: 20px; }}
                    table {{ width: 100%; border-collapse: collapse; margin-top: 10px; }}
                    th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; font-size: 10px; }}
                    th {{ background-color: #f2f2f2; font-weight: bold; }}
                    .text-right {{ text-align: right; }}
                    .total-row {{ background-color: #f9f9f9; font-weight: bold; }}
                    .header-info {{ text-align: center; margin-bottom: 15px; font-size: 12px; color: #666; }}
                </style>
            </head>
            <body>
                <h1>HISTÓRICO - DETALLES DE VENTAS</h1>
                <div class='header-info'>
                    <div>{periodo}</div>
                    <div>Generado: {DateTime.Now:dd/MM/yyyy HH:mm}</div>
                </div>
                
                <table>
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Fecha</th>
                            <th>Cliente</th>
                            <th>Producto</th>
                            <th>Cantidad</th>
                            <th>Precio Unitario</th>
                            <th>Desc. %</th>
                            <th>Monto Desc.</th>
                            <th>Monto Imp.</th>
                            <th>Subtotal</th>
                        </tr>
                    </thead>
                    <tbody>");

            decimal totalSubtotal = 0;
            decimal totalImpuestos = 0;

            foreach (var detalle in ventaDetalles)
            {
                sb.Append($@"
                        <tr>
                            <td>{detalle.Id}</td>
                            <td>{detalle.Venta?.Fecha.ToString("dd/MM/yyyy") ?? ""}</td>
                            <td>{detalle.Venta?.Cliente?.Nombre ?? "N/A"}</td>
                            <td>{detalle.Producto?.Descripcion} ({detalle.Codigo_Producto})</td>
                            <td class='text-right'>{detalle.Cantidad:N0}</td>
                            <td class='text-right'>₡{detalle.Precio_Unitario:N2}</td>
                            <td class='text-right'>{detalle.Porcentaje_Descuento:N2}%</td>
                            <td class='text-right'>₡{detalle.Monto_Descuento:N2}</td>
                            <td class='text-right'>₡{detalle.Monto_Impuesto:N2}</td>
                            <td class='text-right'>₡{detalle.Subtotal:N2}</td>
                        </tr>");

                totalSubtotal += detalle.Subtotal;
                totalImpuestos += detalle.Monto_Impuesto;
            }

            sb.Append($@"
                    </tbody>
                    <tfoot>
                        <tr class='total-row'>
                            <td colspan='8' class='text-right'>TOTAL IMPUESTOS:</td>
                            <td class='text-right'>₡{totalImpuestos:N2}</td>
                            <td></td>
                        </tr>
                        <tr class='total-row'>
                            <td colspan='9' class='text-right'>TOTAL GENERAL:</td>
                            <td class='text-right'>₡{totalSubtotal:N2}</td>
                        </tr>
                    </tfoot>
                </table>
            </body>
            </html>");

            return sb.ToString();
        }

        private static string GenerarHtmlMovimientosInventario(List<Movimiento_Inventario> movimientos, DateTime? fechaInicio, DateTime? fechaFin, string filtroMotivo)
        {
            var sb = new StringBuilder();
            
            // Configurar período y filtros
            string periodo = "";
            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                periodo = $"Período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}";
            }
            else if (fechaInicio.HasValue)
            {
                periodo = $"Desde: {fechaInicio:dd/MM/yyyy}";
            }
            else if (fechaFin.HasValue)
            {
                periodo = $"Hasta: {fechaFin:dd/MM/yyyy}";
            }
            else
            {
                periodo = "Todos los registros";
            }

            if (!string.IsNullOrEmpty(filtroMotivo))
            {
                periodo += $" | Filtro: {filtroMotivo}";
            }

            sb.Append($@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <title>Histórico - Movimientos de Inventario</title>
                <style>
                    body {{ font-family: Arial, sans-serif; margin: 20px; }}
                    h1 {{ color: #333; text-align: center; margin-bottom: 10px; }}
                    h2 {{ color: #666; text-align: center; font-size: 14px; margin-bottom: 20px; }}
                    table {{ width: 100%; border-collapse: collapse; margin-top: 10px; }}
                    th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; font-size: 10px; }}
                    th {{ background-color: #f2f2f2; font-weight: bold; }}
                    .text-right {{ text-align: right; }}
                    .total-row {{ background-color: #f9f9f9; font-weight: bold; }}
                    .header-info {{ text-align: center; margin-bottom: 15px; font-size: 12px; color: #666; }}
                    .entrada {{ color: #28a745; }}
                    .salida {{ color: #dc3545; }}
                    .ajuste {{ color: #ffc107; }}
                </style>
            </head>
            <body>
                <h1>HISTÓRICO - MOVIMIENTOS DE INVENTARIO</h1>
                <div class='header-info'>
                    <div>{periodo}</div>
                    <div>Generado: {DateTime.Now:dd/MM/yyyy HH:mm}</div>
                </div>
                
                <table>
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Fecha</th>
                            <th>Producto</th>
                            <th>Motivo</th>
                            <th>Tipo</th>
                            <th>Cantidad</th>
                            <th>Stock Actual</th>
                            <th>Precio Venta</th>
                        </tr>
                    </thead>
                    <tbody>");

            int totalMovimientos = 0;

            foreach (var movimiento in movimientos)
            {
                var tipoMovimiento = GetTipoMovimiento(movimiento.Motivo);
                var claseColor = GetClaseColor(tipoMovimiento);
                
                sb.Append($@"
                        <tr>
                            <td>{movimiento.Id}</td>
                            <td>{movimiento.Fecha:dd/MM/yyyy HH:mm}</td>
                            <td>{movimiento.Inventario?.Producto?.Descripcion} ({movimiento.Inventario?.Codigo_Producto})</td>
                            <td>{movimiento.Motivo}</td>
                            <td class='{claseColor}'>{tipoMovimiento}</td>
                            <td class='text-right {(movimiento.Cantidad > 0 ? "entrada" : "salida")}'>{(movimiento.Cantidad > 0 ? "+" : "")}{movimiento.Cantidad:N0}</td>
                            <td class='text-right'>{movimiento.Inventario?.Existencia:N0}</td>
                            <td class='text-right'>₡{movimiento.Inventario?.Precio_Venta:N2}</td>
                        </tr>");

                totalMovimientos += Math.Abs(movimiento.Cantidad);
            }

            sb.Append($@"
                    </tbody>
                    <tfoot>
                        <tr class='total-row'>
                            <td colspan='5' class='text-right'>TOTAL MOVIMIENTOS:</td>
                            <td class='text-right'>{totalMovimientos:N0}</td>
                            <td colspan='2'></td>
                        </tr>
                    </tfoot>
                </table>
            </body>
            </html>");

            return sb.ToString();
        }

        private static string GenerarHtmlMovimientosInventarioConStock(List<(Movimiento_Inventario movimiento, int stockHistorico)> movimientosConStock, DateTime? fechaInicio, DateTime? fechaFin, string filtroMotivo)
        {
            var sb = new StringBuilder();
            
            // Configurar período y filtros
            string periodo = "";
            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                periodo = $"Período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}";
            }
            else if (fechaInicio.HasValue)
            {
                periodo = $"Desde: {fechaInicio:dd/MM/yyyy}";
            }
            else if (fechaFin.HasValue)
            {
                periodo = $"Hasta: {fechaFin:dd/MM/yyyy}";
            }
            else
            {
                periodo = "Todos los registros";
            }

            if (!string.IsNullOrEmpty(filtroMotivo))
            {
                periodo += $" | Filtro: {filtroMotivo}";
            }

            sb.Append($@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <title>Histórico - Movimientos de Inventario</title>
                <style>
                    body {{ font-family: Arial, sans-serif; margin: 20px; }}
                    h1 {{ color: #333; text-align: center; margin-bottom: 10px; }}
                    h2 {{ color: #666; text-align: center; font-size: 14px; margin-bottom: 20px; }}
                    table {{ width: 100%; border-collapse: collapse; margin-top: 10px; }}
                    th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; font-size: 10px; }}
                    th {{ background-color: #f2f2f2; font-weight: bold; }}
                    .text-right {{ text-align: right; }}
                    .total-row {{ background-color: #f9f9f9; font-weight: bold; }}
                    .header-info {{ text-align: center; margin-bottom: 15px; font-size: 12px; color: #666; }}
                    .entrada {{ color: #28a745; }}
                    .salida {{ color: #dc3545; }}
                    .ajuste {{ color: #ffc107; }}
                </style>
            </head>
            <body>
                <h1>HISTÓRICO - MOVIMIENTOS DE INVENTARIO</h1>
                <div class='header-info'>
                    <div>{periodo}</div>
                    <div>Generado: {DateTime.Now:dd/MM/yyyy HH:mm}</div>
                </div>
                
                <table>
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Fecha</th>
                            <th>Producto</th>
                            <th>Motivo</th>
                            <th>Tipo</th>
                            <th>Cantidad</th>
                            <th>Stock después del Mov.</th>
                            <th>Precio Venta</th>
                        </tr>
                    </thead>
                    <tbody>");

            int totalMovimientos = 0;

            foreach (var (movimiento, stockHistorico) in movimientosConStock)
            {
                var tipoMovimiento = GetTipoMovimiento(movimiento.Motivo);
                var claseColor = GetClaseColor(tipoMovimiento);
                
                sb.Append($@"
                        <tr>
                            <td>{movimiento.Id}</td>
                            <td>{movimiento.Fecha:dd/MM/yyyy HH:mm}</td>
                            <td>{movimiento.Inventario?.Producto?.Descripcion} ({movimiento.Inventario?.Codigo_Producto})</td>
                            <td>{movimiento.Motivo}</td>
                            <td class='{claseColor}'>{tipoMovimiento}</td>
                            <td class='text-right {(movimiento.Cantidad > 0 ? "entrada" : "salida")}'>{(movimiento.Cantidad > 0 ? "+" : "")}{movimiento.Cantidad:N0}</td>
                            <td class='text-right'>{stockHistorico:N0}</td>
                            <td class='text-right'>₡{movimiento.Inventario?.Precio_Venta:N2}</td>
                        </tr>");

                totalMovimientos += Math.Abs(movimiento.Cantidad);
            }

            sb.Append($@"
                    </tbody>
                    <tfoot>
                        <tr class='total-row'>
                            <td colspan='5' class='text-right'>TOTAL MOVIMIENTOS:</td>
                            <td class='text-right'>{totalMovimientos:N0}</td>
                            <td colspan='2'></td>
                        </tr>
                    </tfoot>
                </table>
            </body>
            </html>");

            return sb.ToString();
        }

        private static string GetTipoMovimiento(string motivo)
        {
            var motivoLower = motivo?.ToLower() ?? "";
            
            if (motivoLower.Contains("venta") || motivoLower.Contains("salida") || motivoLower.Contains("producción"))
                return "Salida";
            else if (motivoLower.Contains("compra") || motivoLower.Contains("entrada") || motivoLower.Contains("ingreso"))
                return "Entrada";
            else if (motivoLower.Contains("ajuste") || motivoLower.Contains("corrección"))
                return "Ajuste";
            else
                return "Otros";
        }

        private static string GetClaseColor(string tipoMovimiento)
        {
            return tipoMovimiento switch
            {
                "Entrada" => "entrada",
                "Salida" => "salida",
                "Ajuste" => "ajuste",
                _ => ""
            };
        }

        private static byte[] GenerarPdfDesdeHtml(string html)
        {
            var converter = new HtmlToPdf();
            
            // Configurar opciones del PDF
            converter.Options.PdfPageSize = PdfPageSize.A4;
            converter.Options.PdfPageOrientation = PdfPageOrientation.Landscape;
            converter.Options.MarginLeft = 10;
            converter.Options.MarginRight = 10;
            converter.Options.MarginTop = 10;
            converter.Options.MarginBottom = 10;

            var doc = converter.ConvertHtmlString(html);
            var result = doc.Save();
            doc.Close();

            return result;
        }
    }
}