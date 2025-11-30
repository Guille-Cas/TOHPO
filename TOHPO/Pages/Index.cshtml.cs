using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TOHPO.Data;

namespace TOHPO.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly AppDbContext _context;

        public IndexModel(ILogger<IndexModel> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public List<PedidoPendienteDto> PedidosPendientes { get; set; } = new();
        public List<EventoProximoDto> EventosProximos { get; set; } = new();
        public List<RecordatorioStickyDto> RecordatoriosSticky { get; set; } = new();

        public async Task OnGetAsync()
        {
            await CargarPedidosPendientes();
            await CargarEventosProximos();
            await CargarRecordatoriosSticky();
        }

        public async Task<IActionResult> OnPostCompletarPedidoAsync(int pedidoId)
        {
            try
            {
                var pedido = await _context.Pedido.FindAsync(pedidoId);
                if (pedido != null)
                {
                    pedido.Estado = true;
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Pedido completado exitosamente";
                }
                else
                {
                    TempData["Error"] = "Pedido no encontrado";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar pedido {PedidoId}", pedidoId);
                TempData["Error"] = "Error al completar el pedido";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCompletarRecordatorioAsync(int recordatorioId)
        {
            try
            {
                // Para esta implementación, vamos a usar las cookies del navegador
                // para recordar qué recordatorios han sido marcados como "listos"
                var recordatoriosOcultos = Request.Cookies["RecordatoriosOcultos"];
                var listaOcultos = new List<string>();
                
                if (!string.IsNullOrEmpty(recordatoriosOcultos))
                {
                    listaOcultos = recordatoriosOcultos.Split(',').ToList();
                }
                
                if (!listaOcultos.Contains(recordatorioId.ToString()))
                {
                    listaOcultos.Add(recordatorioId.ToString());
                }
                
                var opciones = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(7), // Ocultar por 7 días
                    HttpOnly = true
                };
                
                Response.Cookies.Append("RecordatoriosOcultos", string.Join(",", listaOcultos), opciones);
                
                TempData["Success"] = "Recordatorio marcado como listo";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar recordatorio como listo {RecordatorioId}", recordatorioId);
                TempData["Error"] = "Error al procesar el recordatorio";
            }

            return RedirectToPage();
        }

        private async Task CargarPedidosPendientes()
        {
            try
            {
                var hoy = DateTime.Now.Date;
                
                PedidosPendientes = await _context.Pedido
                    .Where(p => !p.Estado)
                    .Include(p => p.Cliente)
                    .OrderBy(p => p.Fecha_Entrega)
                    .Take(5) // Mostrar máximo 5 pedidos pendientes
                    .Select(p => new PedidoPendienteDto
                    {
                        Id = p.Id,
                        Cliente = p.Cliente.Nombre,
                        FechaEntrega = p.Fecha_Entrega,
                        Total = p.Total,
                        EsVencido = p.Fecha_Entrega.Date < hoy,
                        DiasRestantes = (int)(p.Fecha_Entrega.Date - hoy).TotalDays
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar pedidos pendientes");
                PedidosPendientes = new List<PedidoPendienteDto>();
            }
        }

        private async Task CargarEventosProximos()
        {
            try
            {
                var hoy = DateTime.Now.Date;
                var proximoMes = hoy.AddDays(30);
                
                // Obtener recordatorios ocultos de las cookies para filtrar eventos próximos
                var recordatoriosOcultos = Request.Cookies["RecordatoriosOcultos"];
                var listaOcultos = new List<int>();
                
                if (!string.IsNullOrEmpty(recordatoriosOcultos))
                {
                    listaOcultos = recordatoriosOcultos.Split(',')
                        .Where(x => int.TryParse(x, out _))
                        .Select(int.Parse)
                        .ToList();
                }
                
                EventosProximos = await _context.Recordatorio
                    .Where(r => r.Fecha_Hora.Date >= hoy && r.Fecha_Hora.Date <= proximoMes)
                    .Where(r => !listaOcultos.Contains(r.Id)) // Excluir los marcados como listos
                    .Include(r => r.Cliente)
                    .Include(r => r.Motivo_Recordatorio)
                    .OrderBy(r => r.Fecha_Hora)
                    .Take(3) // Mostrar máximo 3 eventos próximos
                    .Select(r => new EventoProximoDto
                    {
                        Id = r.Id,
                        Titulo = r.Motivo_Recordatorio != null ? r.Motivo_Recordatorio.Descripcion : "Evento",
                        Cliente = r.Cliente != null ? r.Cliente.Nombre : "",
                        Detalles = r.Detalles,
                        FechaEvento = r.Fecha_Hora,
                        DiasRestantes = (int)(r.Fecha_Hora.Date - hoy).TotalDays
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar eventos próximos");
                EventosProximos = new List<EventoProximoDto>();
            }
        }

        private async Task CargarRecordatoriosSticky()
        {
            try
            {
                var hoy = DateTime.Now.Date;
                var unaSemanaDelante = hoy.AddDays(7);
                
                // Obtener recordatorios ocultos de las cookies
                var recordatoriosOcultos = Request.Cookies["RecordatoriosOcultos"];
                var listaOcultos = new List<int>();
                
                if (!string.IsNullOrEmpty(recordatoriosOcultos))
                {
                    listaOcultos = recordatoriosOcultos.Split(',')
                        .Where(x => int.TryParse(x, out _))
                        .Select(int.Parse)
                        .ToList();
                }
                
                RecordatoriosSticky = await _context.Recordatorio
                    .Where(r => r.Fecha_Hora.Date >= hoy && r.Fecha_Hora.Date <= unaSemanaDelante)
                    .Where(r => !listaOcultos.Contains(r.Id)) // Excluir los marcados como listos
                    .Include(r => r.Cliente)
                    .Include(r => r.Motivo_Recordatorio)
                    .OrderBy(r => r.Fecha_Hora)
                    .Take(4) // Mostrar máximo 4 sticky notes
                    .Select(r => new RecordatorioStickyDto
                    {
                        Id = r.Id,
                        Titulo = r.Motivo_Recordatorio != null ? r.Motivo_Recordatorio.Descripcion : "Recordatorio",
                        Cliente = r.Cliente != null ? r.Cliente.Nombre : "",
                        Detalles = r.Detalles ?? "",
                        FechaHora = r.Fecha_Hora,
                        DiasRestantes = (int)(r.Fecha_Hora.Date - hoy).TotalDays,
                        ColorSticky = ObtenerColorSticky(r.Id) // Para variedad visual
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar recordatorios sticky");
                RecordatoriosSticky = new List<RecordatorioStickyDto>();
            }
        }

        private string ObtenerColorSticky(int recordatorioId)
        {
            // Asignar colores basados en el ID para consistencia
            var colores = new[] { "yellow", "pink", "blue", "green", "orange" };
            return colores[recordatorioId % colores.Length];
        }
    }

    public class PedidoPendienteDto
    {
        public int Id { get; set; }
        public string Cliente { get; set; } = "";
        public DateTime FechaEntrega { get; set; }
        public decimal Total { get; set; }
        public bool EsVencido { get; set; }
        public int DiasRestantes { get; set; }
    }

    public class EventoProximoDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public string Cliente { get; set; } = "";
        public string? Detalles { get; set; }
        public DateTime FechaEvento { get; set; }
        public int DiasRestantes { get; set; }
    }

    public class RecordatorioStickyDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public string Cliente { get; set; } = "";
        public string Detalles { get; set; } = "";
        public DateTime FechaHora { get; set; }
        public int DiasRestantes { get; set; }
        public string ColorSticky { get; set; } = "yellow";
    }
}
