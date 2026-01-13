using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TOHPO.Data;
using TOHPO.Models;
using TOHPO.Helpers;

namespace TOHPO.Pages.ControlHistorico.DetalleInventario
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
        public string BuscarMotivo { get; set; } = string.Empty;

        public List<MovimientoInventarioConStock> MovimientosInventario { get; set; } = new List<MovimientoInventarioConStock>();
        public int TotalMovimientos { get; set; }

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
            
            var movimientosConStock = MovimientosInventario.Select(m => (m.Movimiento, m.StockDespuesDelMovimiento)).ToList();
            var excel = ExcelReportHelper.GenerarReporteMovimientosInventarioConStockExcel(
                movimientosConStock, 
                FechaInicio, 
                FechaFin, 
                BuscarMotivo);

            var fileName = $"Historico_Movimientos_Inventario_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public async Task<IActionResult> OnPostExportarPdfAsync()
        {
            await CargarDatos();
            
            var movimientosConStock = MovimientosInventario.Select(m => (m.Movimiento, m.StockDespuesDelMovimiento)).ToList();
            var pdf = PdfReportHelper.GenerarReporteMovimientosInventarioConStockPdf(
                movimientosConStock, 
                FechaInicio, 
                FechaFin, 
                BuscarMotivo);

            var fileName = $"Historico_Movimientos_Inventario_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return File(pdf, "application/pdf", fileName);
        }

        public async Task<IActionResult> OnPostCrearMovimientosFaltantesAsync()
        {
            try
            {
                int movimientosCreados = 0;

                // 1. Obtener todas las ventas y sus IDs que ya tienen movimientos
                var ventasConMovimientos = await _context.Movimiento_Inventario
                    .Where(mi => mi.Motivo.Contains("Venta #"))
                    .Select(mi => mi.Motivo)
                    .ToListAsync();

                var idsVentasConMovimientos = ventasConMovimientos
                    .Where(motivo => motivo.StartsWith("Venta #"))
                    .Select(motivo => {
                        var parts = motivo.Split('#');
                        if (parts.Length > 1)
                        {
                            var idPart = parts[1].Split(' ')[0].Split('-')[0];
                            if (int.TryParse(idPart, out int id))
                                return id;
                        }
                        return -1;
                    })
                    .Where(id => id > 0)
                    .Distinct()
                    .ToHashSet();

                // Buscar ventas que NO tienen movimientos registrados
                var todasLasVentas = await _context.Venta
                    .Include(v => v.Detalle_Ventas)
                        .ThenInclude(dv => dv.Producto)
                    .OrderBy(v => v.Fecha)
                    .ThenBy(v => v.Hora)
                    .ToListAsync();

                var ventasSinMovimientos = todasLasVentas
                    .Where(v => !idsVentasConMovimientos.Contains(v.Id))
                    .ToList();

                foreach (var venta in ventasSinMovimientos)
                {
                    foreach (var detalle in venta.Detalle_Ventas)
                    {
                        var inventario = await _context.Inventario
                            .FirstOrDefaultAsync(i => i.Codigo_Producto == detalle.Codigo_Producto);

                        if (inventario != null)
                        {
                            var movimiento = new Movimiento_Inventario
                            {
                                Id_Inventario = inventario.Id,
                                Cantidad = -detalle.Cantidad, // Negativo para ventas (salida)
                                Motivo = $"Venta #{venta.Id} - {detalle.Producto?.Descripcion ?? detalle.Codigo_Producto} (Generado automáticamente)",
                                Fecha = venta.Hora
                            };

                            _context.Movimiento_Inventario.Add(movimiento);
                            movimientosCreados++;
                        }
                    }
                }

                // 2. Hacer lo mismo para compras
                var comprasConMovimientos = await _context.Movimiento_Inventario
                    .Where(mi => mi.Motivo.Contains("Compra #"))
                    .Select(mi => mi.Motivo)
                    .ToListAsync();

                var idsComprasConMovimientos = comprasConMovimientos
                    .Where(motivo => motivo.StartsWith("Compra #"))
                    .Select(motivo => {
                        var parts = motivo.Split('#');
                        if (parts.Length > 1)
                        {
                            var idPart = parts[1].Split(' ')[0].Split('-')[0];
                            if (int.TryParse(idPart, out int id))
                                return id;
                        }
                        return -1;
                    })
                    .Where(id => id > 0)
                    .Distinct()
                    .ToHashSet();

                // Buscar compras que NO tienen movimientos registrados
                var todasLasCompras = await _context.Compra
                    .Include(c => c.Compra_Detalles)
                        .ThenInclude(cd => cd.Producto)
                    .OrderBy(c => c.Fecha)
                    .ThenBy(c => c.Hora)
                    .ToListAsync();

                var comprasSinMovimientos = todasLasCompras
                    .Where(c => !idsComprasConMovimientos.Contains(c.Id))
                    .ToList();

                foreach (var compra in comprasSinMovimientos)
                {
                    foreach (var detalle in compra.Compra_Detalles)
                    {
                        var inventario = await _context.Inventario
                            .FirstOrDefaultAsync(i => i.Codigo_Producto == detalle.Codigo_Producto);

                        if (inventario != null)
                        {
                            var movimiento = new Movimiento_Inventario
                            {
                                Id_Inventario = inventario.Id,
                                Cantidad = detalle.Cantidad, // Positivo para compras (entrada)
                                Motivo = $"Compra #{compra.Id} - {detalle.Producto?.Descripcion ?? detalle.Codigo_Producto} (Generado automáticamente)",
                                Fecha = compra.Hora
                            };

                            _context.Movimiento_Inventario.Add(movimiento);
                            movimientosCreados++;
                        }
                    }
                }

                if (movimientosCreados > 0)
                {
                    await _context.SaveChangesAsync();
                }
                
                TempData["SuccessMessage"] = $"Se crearon {movimientosCreados} movimientos de inventario faltantes (ventas y compras)";
                
                // Recargar datos para mostrar los nuevos movimientos
                await CargarDatos();
                
                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al crear movimientos faltantes: {ex.Message}";
                return Page();
            }
        }

        private async Task CargarDatos()
        {
            var query = _context.Movimiento_Inventario
                .Include(mi => mi.Inventario)
                    .ThenInclude(i => i.Producto)
                .AsQueryable();

            // Aplicar filtros
            if (FechaInicio.HasValue)
            {
                query = query.Where(mi => mi.Fecha >= FechaInicio.Value);
            }

            if (FechaFin.HasValue)
            {
                query = query.Where(mi => mi.Fecha <= FechaFin.Value);
            }

            if (!string.IsNullOrWhiteSpace(BuscarMotivo))
            {
                query = query.Where(mi => mi.Motivo.Contains(BuscarMotivo));
            }

            var movimientosBasicos = await query
                .OrderByDescending(mi => mi.Fecha)
                .ThenBy(mi => mi.Id)
                .ToListAsync();

            // Calcular stock histórico para cada movimiento
            MovimientosInventario = await CalcularStockHistorico(movimientosBasicos);

            // Calcular totales
            TotalMovimientos = MovimientosInventario.Sum(mi => Math.Abs(mi.Movimiento.Cantidad));
        }

        private async Task<List<MovimientoInventarioConStock>> CalcularStockHistorico(List<Movimiento_Inventario> movimientos)
        {
            var resultado = new List<MovimientoInventarioConStock>();

            if (!movimientos.Any())
                return resultado;

            // Agrupar por inventario para calcular stocks históricos
            var movimientosPorInventario = movimientos.GroupBy(m => m.Id_Inventario);

            foreach (var grupo in movimientosPorInventario)
            {
                var inventarioId = grupo.Key;
                var movimientosDelInventario = grupo.OrderBy(m => m.Fecha).ThenBy(m => m.Id).ToList();
                
                // Obtener TODOS los movimientos de este inventario hasta el más reciente filtrado
                var fechaHasta = movimientosDelInventario.Max(m => m.Fecha);
                var todosLosMovimientos = await _context.Movimiento_Inventario
                    .Where(m => m.Id_Inventario == inventarioId && m.Fecha <= fechaHasta)
                    .OrderBy(m => m.Fecha)
                    .ThenBy(m => m.Id)
                    .ToListAsync();

                // Obtener el stock actual del inventario
                var stockActual = movimientosDelInventario.First().Inventario?.Existencia ?? 0;
                
                // Calcular el stock al momento del último movimiento considerado
                var movimientosPosteriors = await _context.Movimiento_Inventario
                    .Where(m => m.Id_Inventario == inventarioId && m.Fecha > fechaHasta)
                    .SumAsync(m => m.Cantidad);
                
                var stockEnFechaHasta = stockActual - movimientosPosteriors;
                
                // Calcular stock inicial (antes del primer movimiento considerado)
                var totalCambiosConsiderados = todosLosMovimientos.Sum(m => m.Cantidad);
                var stockInicial = stockEnFechaHasta - totalCambiosConsiderados;
                
                var stockCalculado = stockInicial;
                
                // Calcular el stock después de cada movimiento
                foreach (var movimiento in todosLosMovimientos)
                {
                    stockCalculado += movimiento.Cantidad;
                    
                    // Solo incluir si está en nuestro resultado filtrado
                    if (movimientosDelInventario.Any(m => m.Id == movimiento.Id))
                    {
                        resultado.Add(new MovimientoInventarioConStock
                        {
                            Movimiento = movimiento,
                            StockDespuesDelMovimiento = stockCalculado
                        });
                    }
                }
            }

            // Retornar ordenado como estaba originalmente (más reciente primero)
            return resultado.OrderByDescending(r => r.Movimiento.Fecha)
                          .ThenBy(r => r.Movimiento.Id)
                          .ToList();
        }
    }

    public class MovimientoInventarioConStock
    {
        public Movimiento_Inventario Movimiento { get; set; }
        public int StockDespuesDelMovimiento { get; set; }
    }
}