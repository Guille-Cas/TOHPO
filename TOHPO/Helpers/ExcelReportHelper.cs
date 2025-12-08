using ClosedXML.Excel;
using System.Globalization;
using TOHPO.Models;

namespace TOHPO.Helpers
{
    public static class ExcelReportHelper
    {
        // Cultura costarricense para formateo de moneda
        private static readonly CultureInfo _culturaCostaRica = new("es-CR");
        
        // Formato de colones para Excel
        private const string FORMATO_COLONES = "₡#,##0.00";

        public static byte[] GenerarReporteVentasExcel(List<Venta> ventas, DateTime fechaInicio, DateTime fechaFin)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Reporte de Ventas");

            // Configurar título
            worksheet.Cell(1, 1).Value = "REPORTE DE VENTAS";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Range(1, 1, 1, 8).Merge();

            // Configurar período
            worksheet.Cell(2, 1).Value = $"Período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}";
            worksheet.Cell(2, 1).Style.Font.Bold = true;
            worksheet.Range(2, 1, 2, 8).Merge();

            // Headers
            var headers = new string[] 
            { 
                "ID", "Fecha", "Cliente", "Concepto", "Subtotal", "IVA", "Total", "Métodos de Pago" 
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(4, i + 1).Value = headers[i];
                worksheet.Cell(4, i + 1).Style.Font.Bold = true;
                worksheet.Cell(4, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Cell(4, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // Datos
            int row = 5;
            decimal totalGeneral = 0;

            foreach (var venta in ventas)
            {
                worksheet.Cell(row, 1).Value = venta.Id;
                worksheet.Cell(row, 2).Value = venta.Fecha.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 3).Value = venta.Cliente?.Nombre ?? "N/A";
                worksheet.Cell(row, 4).Value = venta.Concepto ?? "";
                worksheet.Cell(row, 5).Value = venta.Costo_Total_Gravado;
                worksheet.Cell(row, 6).Value = venta.Iva;
                worksheet.Cell(row, 7).Value = venta.Total;

                // Métodos de pago con formato de colones
                var metodosPago = string.Join(", ", 
                    venta.Venta_Metodo_Pagos?.Select(mp => 
                        $"{mp.Metodo_Pago?.Descripcion}: {mp.Monto.ToString("C", _culturaCostaRica)}") 
                    ?? new List<string>());
                worksheet.Cell(row, 8).Value = metodosPago;

                totalGeneral += venta.Total;
                row++;
            }

            // Total general
            worksheet.Cell(row + 1, 6).Value = "TOTAL GENERAL:";
            worksheet.Cell(row + 1, 6).Style.Font.Bold = true;
            worksheet.Cell(row + 1, 7).Value = totalGeneral;
            worksheet.Cell(row + 1, 7).Style.Font.Bold = true;
            worksheet.Cell(row + 1, 7).Style.NumberFormat.Format = FORMATO_COLONES;

            // Formatear monedas en colones
            worksheet.Range(5, 5, row - 1, 7).Style.NumberFormat.Format = FORMATO_COLONES;

            // Auto-ajustar columnas
            worksheet.Columns().AdjustToContents();

            // Generar archivo
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public static byte[] GenerarReporteComprasExcel(List<Compra> compras, DateTime fechaInicio, DateTime fechaFin)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Reporte de Compras");

            // Configurar título
            worksheet.Cell(1, 1).Value = "REPORTE DE COMPRAS";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Range(1, 1, 1, 8).Merge();

            // Configurar período
            worksheet.Cell(2, 1).Value = $"Período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}";
            worksheet.Cell(2, 1).Style.Font.Bold = true;
            worksheet.Range(2, 1, 2, 8).Merge();

            // Headers
            var headers = new string[] 
            { 
                "ID", "Fecha", "Proveedor", "Subtotal", "IVA", "Total", "Métodos de Pago", "Estado" 
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(4, i + 1).Value = headers[i];
                worksheet.Cell(4, i + 1).Style.Font.Bold = true;
                worksheet.Cell(4, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Cell(4, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // Datos
            int row = 5;
            decimal totalGeneral = 0;

            foreach (var compra in compras)
            {
                worksheet.Cell(row, 1).Value = compra.Id;
                worksheet.Cell(row, 2).Value = compra.Fecha.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 3).Value = compra.Proveedor?.Nombre ?? "N/A";
                worksheet.Cell(row, 4).Value = compra.Costo_Total_Grabado;
                worksheet.Cell(row, 5).Value = compra.Iva;
                worksheet.Cell(row, 6).Value = compra.Total;

                // Métodos de pago con formato de colones
                var metodosPago = string.Join(", ", 
                    compra.Compra_Metodo_Pagos?.Select(mp => 
                        $"{mp.Metodo_Pago?.Descripcion}: {mp.Monto.ToString("C", _culturaCostaRica)}") 
                    ?? new List<string>());
                worksheet.Cell(row, 7).Value = metodosPago;
                worksheet.Cell(row, 8).Value = compra.Total.ToString() ?? "";

                totalGeneral += compra.Total;
                row++;
            }

            // Total general
            worksheet.Cell(row + 1, 5).Value = "TOTAL GENERAL:";
            worksheet.Cell(row + 1, 5).Style.Font.Bold = true;
            worksheet.Cell(row + 1, 6).Value = totalGeneral;
            worksheet.Cell(row + 1, 6).Style.Font.Bold = true;
            worksheet.Cell(row + 1, 6).Style.NumberFormat.Format = FORMATO_COLONES;

            // Formatear monedas en colones
            worksheet.Range(5, 4, row - 1, 6).Style.NumberFormat.Format = FORMATO_COLONES;

            // Auto-ajustar columnas
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public static byte[] GenerarReporteInventarioExcel(List<Inventario> inventario)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Inventario");

            // Título
            worksheet.Cell(1, 1).Value = "REPORTE DE INVENTARIO";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Range(1, 1, 1, 7).Merge();

            worksheet.Cell(2, 1).Value = $"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}";
            worksheet.Range(2, 1, 2, 7).Merge();

            // Headers
            var headers = new string[] 
            { 
                "Producto", "Categoría", "Presentación", "Existencia", "Precio Unitario", "Valor Total", "Estado" 
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(4, i + 1).Value = headers[i];
                worksheet.Cell(4, i + 1).Style.Font.Bold = true;
                worksheet.Cell(4, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Cell(4, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // Datos
            int row = 5;
            decimal valorTotalInventario = 0;

            foreach (var item in inventario)
            {
                var precioVenta = item.Precio_Venta;
                var valorTotal = item.Existencia * precioVenta;
                
                worksheet.Cell(row, 1).Value = item.Producto?.Descripcion ?? "N/A";
                worksheet.Cell(row, 2).Value = item.Producto?.Categoria?.Descripcion ?? "N/A";
                worksheet.Cell(row, 3).Value = $"{item.Producto?.Presentacion?.Cantidad ?? 0} {item.Producto?.Presentacion?.Unidad_Medida.ToString() ?? ""}";
                worksheet.Cell(row, 4).Value = item.Existencia;
                worksheet.Cell(row, 5).Value = precioVenta;
                worksheet.Cell(row, 6).Value = valorTotal;
                worksheet.Cell(row, 7).Value = item.Existencia > 0 ? "Disponible" : "Agotado";

                valorTotalInventario += valorTotal;
                row++;
            }

            // Total inventario
            worksheet.Cell(row + 1, 5).Value = "VALOR TOTAL INVENTARIO:";
            worksheet.Cell(row + 1, 5).Style.Font.Bold = true;
            worksheet.Cell(row + 1, 6).Value = valorTotalInventario;
            worksheet.Cell(row + 1, 6).Style.Font.Bold = true;
            worksheet.Cell(row + 1, 6).Style.NumberFormat.Format = FORMATO_COLONES;

            // Formatear columnas de precio en colones
            worksheet.Range(5, 5, row - 1, 6).Style.NumberFormat.Format = FORMATO_COLONES;

            // Auto-ajustar columnas
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public static byte[] GenerarReportePedidosExcel(List<Pedido> pedidos, DateTime fechaInicio, DateTime fechaFin)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Reporte de Pedidos");

            // Configurar título
            worksheet.Cell(1, 1).Value = "REPORTE DE PEDIDOS";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Range(1, 1, 1, 10).Merge();

            // Configurar período
            worksheet.Cell(2, 1).Value = $"Período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}";
            worksheet.Cell(2, 1).Style.Font.Bold = true;
            worksheet.Range(2, 1, 2, 10).Merge();

            worksheet.Cell(3, 1).Value = $"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}";
            worksheet.Range(3, 1, 3, 10).Merge();

            // Headers principales
            var headers = new string[] 
            { 
                "ID", "Fecha Creación", "Fecha Entrega", "Cliente", "Total", "Abono", "Saldo", "Estado", "Días Pendientes", "Métodos de Pago" 
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(5, i + 1).Value = headers[i];
                worksheet.Cell(5, i + 1).Style.Font.Bold = true;
                worksheet.Cell(5, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Cell(5, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(5, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Datos principales de pedidos
            int row = 6;
            decimal totalPedidos = 0;
            decimal totalAbonos = 0;
            decimal totalSaldos = 0;
            int pedidosActivos = 0;

            foreach (var pedido in pedidos.OrderBy(p => p.Fecha_Creacion))
            {
                // Calcular días pendientes
                var diasPendientes = pedido.Estado && pedido.Saldo > 0 
                    ? (DateTime.Now.Date - pedido.Fecha_Entrega.Date).Days 
                    : 0;

                // Métodos de pago con formato de colones
                var metodosPago = string.Join(", ", 
                    pedido.Pedido_Metodo_Pagos?.Select(mp => 
                        $"{mp.Metodo_Pago?.Descripcion}: {mp.Monto.ToString("C", _culturaCostaRica)}") 
                    ?? new List<string>());

                worksheet.Cell(row, 1).Value = pedido.Id;
                worksheet.Cell(row, 2).Value = pedido.Fecha_Creacion.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 3).Value = pedido.Fecha_Entrega.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 4).Value = pedido.Cliente?.Nombre ?? "N/A";
                worksheet.Cell(row, 5).Value = pedido.Total;
                worksheet.Cell(row, 6).Value = pedido.Abono;
                worksheet.Cell(row, 7).Value = pedido.Saldo;
                worksheet.Cell(row, 8).Value = pedido.Estado ? "Activo" : "Completado";
                worksheet.Cell(row, 9).Value = diasPendientes > 0 ? diasPendientes : 0;
                worksheet.Cell(row, 10).Value = string.IsNullOrEmpty(metodosPago) ? "Sin pagos" : metodosPago;

                // Formato condicional para días pendientes
                if (diasPendientes > 7)
                {
                    worksheet.Cell(row, 9).Style.Fill.BackgroundColor = XLColor.LightCoral;
                    worksheet.Cell(row, 9).Style.Font.Bold = true;
                }
                else if (diasPendientes > 0)
                {
                    worksheet.Cell(row, 9).Style.Fill.BackgroundColor = XLColor.LightYellow;
                }

                // Formato condicional para estado
                if (pedido.Estado && pedido.Saldo > 0)
                {
                    worksheet.Cell(row, 8).Style.Fill.BackgroundColor = XLColor.LightYellow;
                }
                else if (!pedido.Estado || pedido.Saldo == 0)
                {
                    worksheet.Cell(row, 8).Style.Fill.BackgroundColor = XLColor.LightGreen;
                }

                totalPedidos += pedido.Total;
                totalAbonos += pedido.Abono;
                totalSaldos += pedido.Saldo;
                if (pedido.Estado) pedidosActivos++;

                row++;
            }

            // Totales generales
            worksheet.Cell(row + 1, 4).Value = "TOTALES GENERALES:";
            worksheet.Cell(row + 1, 4).Style.Font.Bold = true;
            worksheet.Cell(row + 1, 5).Value = totalPedidos;
            worksheet.Cell(row + 1, 6).Value = totalAbonos;
            worksheet.Cell(row + 1, 7).Value = totalSaldos;

            // Estadísticas adicionales
            worksheet.Cell(row + 3, 1).Value = "ESTADÍSTICAS:";
            worksheet.Cell(row + 3, 1).Style.Font.Bold = true;
            worksheet.Cell(row + 3, 1).Style.Font.FontSize = 14;

            worksheet.Cell(row + 4, 1).Value = "Total de pedidos:";
            worksheet.Cell(row + 4, 2).Value = pedidos.Count;
            worksheet.Cell(row + 5, 1).Value = "Pedidos activos:";
            worksheet.Cell(row + 5, 2).Value = pedidosActivos;
            worksheet.Cell(row + 6, 1).Value = "Pedidos completados:";
            worksheet.Cell(row + 6, 2).Value = pedidos.Count - pedidosActivos;
            worksheet.Cell(row + 7, 1).Value = "Porcentaje de cumplimiento:";
            worksheet.Cell(row + 7, 2).Value = pedidos.Count > 0 ? $"{((pedidos.Count - pedidosActivos) * 100.0 / pedidos.Count):F1}%" : "0%";

            // Crear hoja adicional con detalle de productos
            var detailWorksheet = workbook.Worksheets.Add("Detalle de Productos");
            GenerarDetalleProductosPedidos(detailWorksheet, pedidos);

            // Formatear monedas en colones en la hoja principal
            worksheet.Range(6, 5, row - 1, 7).Style.NumberFormat.Format = FORMATO_COLONES;
            worksheet.Range(row + 1, 5, row + 1, 7).Style.NumberFormat.Format = FORMATO_COLONES;
            worksheet.Range(row + 1, 5, row + 1, 7).Style.Font.Bold = true;

            // Auto-ajustar columnas
            worksheet.Columns().AdjustToContents();

            // Generar archivo
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static void GenerarDetalleProductosPedidos(IXLWorksheet worksheet, List<Pedido> pedidos)
        {
            // Título
            worksheet.Cell(1, 1).Value = "DETALLE DE PRODUCTOS EN PEDIDOS";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 14;
            worksheet.Range(1, 1, 1, 7).Merge();

            // Headers
            var headers = new string[] 
            { 
                "ID Pedido", "Cliente", "Producto", "Cantidad", "Precio Unitario", "Subtotal", "Estado Pedido" 
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(3, i + 1).Value = headers[i];
                worksheet.Cell(3, i + 1).Style.Font.Bold = true;
                worksheet.Cell(3, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
                worksheet.Cell(3, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // Datos de detalles
            int row = 4;
            decimal totalProductos = 0;
            int cantidadTotal = 0;

            foreach (var pedido in pedidos.OrderBy(p => p.Id))
            {
                if (pedido.Pedido_Detalles != null && pedido.Pedido_Detalles.Any())
                {
                    foreach (var detalle in pedido.Pedido_Detalles)
                    {
                        var subtotal = detalle.Cantidad * detalle.Precio_Unitario;

                        worksheet.Cell(row, 1).Value = pedido.Id;
                        worksheet.Cell(row, 2).Value = pedido.Cliente?.Nombre ?? "N/A";
                        worksheet.Cell(row, 3).Value = detalle.Producto?.Descripcion ?? detalle.Codigo_Producto ?? "N/A";
                        worksheet.Cell(row, 4).Value = detalle.Cantidad;
                        worksheet.Cell(row, 5).Value = detalle.Precio_Unitario;
                        worksheet.Cell(row, 6).Value = subtotal;
                        worksheet.Cell(row, 7).Value = pedido.Estado ? "Activo" : "Completado";

                        totalProductos += subtotal;
                        cantidadTotal += detalle.Cantidad;
                        row++;
                    }
                }
            }

            // Totales
            worksheet.Cell(row + 1, 3).Value = "TOTALES:";
            worksheet.Cell(row + 1, 3).Style.Font.Bold = true;
            worksheet.Cell(row + 1, 4).Value = cantidadTotal;
            worksheet.Cell(row + 1, 6).Value = totalProductos;

            // Formatear monedas en colones
            worksheet.Range(4, 5, row - 1, 6).Style.NumberFormat.Format = FORMATO_COLONES;
            worksheet.Range(row + 1, 6, row + 1, 6).Style.NumberFormat.Format = FORMATO_COLONES;
            worksheet.Range(row + 1, 3, row + 1, 6).Style.Font.Bold = true;

            // Auto-ajustar columnas
            worksheet.Columns().AdjustToContents();
        }
    }
}