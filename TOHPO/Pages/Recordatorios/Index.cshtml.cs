using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Data;
using TOHPO.Models;
using Microsoft.EntityFrameworkCore;
using TOHPO.Models.Enums;

namespace TOHPO.Pages.Recordatorios
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public IndexModel(AppDbContext context) { _context = context; }
        
        public void OnGet() { }

        public JsonResult OnGetRecordatorios()
        {
            try
            {
                var recordatorios = _context.Recordatorio
                    .Include(r => r.Cliente)
                    .Include(r => r.Motivo_Recordatorio)
                    .Include(r => r.RecordatorioPadre)
                    .Select(r => new {
                        id = r.Id,
                        fecha_Hora = r.Fecha_Hora.ToString("dd/MM/yyyy HH:mm"),
                        cliente = r.Cliente != null ? $"{r.Cliente.Nombre} {r.Cliente.Primer_Apellido} {r.Cliente.Segundo_Apellido}".Trim() : "",
                        motivo = r.Motivo_Recordatorio != null ? r.Motivo_Recordatorio.Descripcion : "",
                        detalles = r.Detalles ?? "",
                        recurrencia = r.EsRecurrente ? GetRecurrenciaTexto(r.TipoRecurrencia, r.IntervaloRecurrencia) : 
                                     (r.RecordatorioPadreId != null ? "Parte de serie recurrente" : "No recurrente"),
                        clienteId = r.ClienteId,
                        motivoId = r.Motivo_RecordatorioId,
                        fechaCompleta = r.Fecha_Hora,
                        esRecurrente = r.EsRecurrente,
                        recordatorioPadreId = r.RecordatorioPadreId
                    })
                    .OrderBy(r => r.fechaCompleta)
                    .ToList();
                
                return new JsonResult(recordatorios);
            }
            catch (Exception ex)
            {
                // Log del error para debugging
                Console.WriteLine($"Error al cargar recordatorios: {ex.Message}");
                return new JsonResult(new { error = ex.Message });
            }
        }

        public JsonResult OnGetCalendarEvents()
        {
            try
            {
                var eventos = _context.Recordatorio
                    .Include(r => r.Cliente)
                    .Include(r => r.Motivo_Recordatorio)
                    .Select(r => new {
                        id = r.Id,
                        title = $"{r.Cliente.Nombre} - {r.Motivo_Recordatorio.Descripcion}",
                        start = r.Fecha_Hora.ToString("yyyy-MM-ddTHH:mm:ss"),
                        description = r.Detalles,
                        backgroundColor = r.Fecha_Hora < DateTime.Now ? "#dc3545" : 
                                        (r.EsRecurrente || r.RecordatorioPadreId != null ? "#28a745" : "#007bff"),
                        borderColor = r.EsRecurrente || r.RecordatorioPadreId != null ? "#1e7e34" : "#0056b3",
                        textColor = "#ffffff"
                    })
                    .ToList();
                
                return new JsonResult(eventos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar eventos del calendario: {ex.Message}");
                return new JsonResult(new { error = ex.Message });
            }
        }

        public IActionResult OnPostEliminar([FromForm] int id)
        {
            try
            {
                var recordatorio = _context.Recordatorio
                    .Include(r => r.RecordatoriosHijo)
                    .FirstOrDefault(r => r.Id == id);
                
                if (recordatorio == null) 
                    return new JsonResult(new { success = false, message = "Recordatorio no encontrado" });
                
                // Si es un recordatorio recurrente (padre), eliminar también los hijos
                if (recordatorio.EsRecurrente && recordatorio.RecordatoriosHijo.Any())
                {
                    _context.Recordatorio.RemoveRange(recordatorio.RecordatoriosHijo);
                }
                
                _context.Recordatorio.Remove(recordatorio);
                _context.SaveChanges();
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar recordatorio: {ex.Message}");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public JsonResult OnGetDetalles(int id)
        {
            try
            {
                var recordatorio = _context.Recordatorio
                    .Include(r => r.Cliente)
                    .Include(r => r.Motivo_Recordatorio)
                    .Include(r => r.RecordatorioPadre)
                    .Include(r => r.RecordatoriosHijo)
                    .FirstOrDefault(r => r.Id == id);

                if (recordatorio == null)
                    return new JsonResult(new { error = "Recordatorio no encontrado" });

                var detalles = new
                {
                    id = recordatorio.Id,
                    fechaHora = recordatorio.Fecha_Hora.ToString("dddd, dd 'de' MMMM 'de' yyyy 'a las' HH:mm", new System.Globalization.CultureInfo("es-ES")),
                    cliente = $"{recordatorio.Cliente?.Nombre} {recordatorio.Cliente?.Primer_Apellido} {recordatorio.Cliente?.Segundo_Apellido}".Trim(),
                    clienteTelefono = recordatorio.Cliente?.Telefono,
                    clienteEmail = recordatorio.Cliente?.Correo_Electronico,
                    motivo = recordatorio.Motivo_Recordatorio?.Descripcion,
                    detalles = recordatorio.Detalles,
                    esRecurrente = recordatorio.EsRecurrente,
                    tipoRecurrencia = recordatorio.TipoRecurrencia?.ToString(),
                    intervaloRecurrencia = recordatorio.IntervaloRecurrencia,
                    fechaFinRecurrencia = recordatorio.FechaFinRecurrencia?.ToString("dd/MM/yyyy"),
                    maximoRepeticiones = recordatorio.MaximoRepeticiones,
                    esParteDeSerie = recordatorio.RecordatorioPadreId != null,
                    numeroDeHijos = recordatorio.RecordatoriosHijo.Count,
                    estado = GetEstadoRecordatorio(recordatorio.Fecha_Hora)
                };

                return new JsonResult(detalles);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.Message });
            }
        }

        private static string GetRecurrenciaTexto(TipoRecurrencia? tipo, int intervalo)
        {
            if (!tipo.HasValue) return "No recurrente";

            var texto = tipo.Value switch
            {
                TipoRecurrencia.Diario => intervalo == 1 ? "Diario" : $"Cada {intervalo} días",
                TipoRecurrencia.Semanal => intervalo == 1 ? "Semanal" : $"Cada {intervalo} semanas",
                TipoRecurrencia.Mensual => intervalo == 1 ? "Mensual" : $"Cada {intervalo} meses",
                TipoRecurrencia.Anual => intervalo == 1 ? "Anual" : $"Cada {intervalo} años",
                _ => "Desconocido"
            };

            return texto;
        }

        private static string GetEstadoRecordatorio(DateTime fechaHora)
        {
            var ahora = DateTime.Now;
            var diferencia = fechaHora - ahora;

            if (diferencia.TotalDays < 0)
                return "Vencido";
            else if (diferencia.TotalHours <= 24)
                return "Próximo";
            else
                return "Programado";
        }
    }
}