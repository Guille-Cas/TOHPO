using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TOHPO.Data;
using TOHPO.Models;

namespace TOHPO.Pages.Configuracion.Motivo_Recordatorio
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
        }

        // Handler para DataTables
        public JsonResult OnGetMotivos()
        {
            try
            {
                var motivos = _context.Motivo_Recordatorio
                    .Select(m => new {
                        id = m.Id,
                        descripcion = m.Descripcion
                    }).ToList();

                return new JsonResult(motivos);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.Message });
            }
        }

        // Handler para eliminar un motivo
        public IActionResult OnPostEliminar([FromForm] int id)
        {
            try
            {
                var motivo = _context.Motivo_Recordatorio.Find(id);
                if (motivo == null)
                    return new JsonResult(new { success = false, message = "Motivo no encontrado" });

                // Verificar si el motivo está siendo usado en recordatorios
                var recordatoriosConMotivo = _context.Recordatorio
                    .Any(r => r.Motivo_RecordatorioId == id);

                if (recordatoriosConMotivo)
                {
                    return new JsonResult(new { success = false, message = "No se puede eliminar el motivo porque está siendo usado en recordatorios existentes." });
                }

                _context.Motivo_Recordatorio.Remove(motivo);
                _context.SaveChanges();

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }
    }
}