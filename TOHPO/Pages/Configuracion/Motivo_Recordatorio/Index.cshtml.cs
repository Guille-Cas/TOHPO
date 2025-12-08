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
        public async Task<JsonResult> OnGetMotivosAsync()
        {
            try
            {
                var motivos = await _context.Motivo_Recordatorio
                    .OrderBy(m => m.Descripcion)
                    .Select(m => new {
                        id = m.Id,
                        descripcion = m.Descripcion,
                        estado = m.Estado
                    })
                    .ToListAsync();

                return new JsonResult(motivos);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al cargar motivos: {ex.Message}" });
            }
        }

        // Handler para crear un motivo
        public async Task<IActionResult> OnPostCrearAsync([FromForm] string descripcion)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(descripcion))
                {
                    return new JsonResult(new { success = false, message = "La descripción es requerida", type = "error" });
                }

                // Verificar si ya existe un motivo con la misma descripción
                var motivoExistente = await _context.Motivo_Recordatorio
                    .AnyAsync(m => m.Descripcion.ToLower() == descripcion.ToLower());

                if (motivoExistente)
                {
                    return new JsonResult(new { success = false, message = "Ya existe un motivo con esa descripción", type = "error" });
                }

                var nuevoMotivo = new TOHPO.Models.Motivo_Recordatorio
                {
                    Descripcion = descripcion.Trim(),
                    Estado = true
                };

                _context.Motivo_Recordatorio.Add(nuevoMotivo);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Motivo creado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al crear motivo: {ex.Message}", type = "error" });
            }
        }

        // Handler para editar un motivo
        public async Task<IActionResult> OnPostEditarAsync([FromForm] int id, [FromForm] string descripcion, [FromForm] bool estado)
        {
            try
            {
                var motivo = await _context.Motivo_Recordatorio.FindAsync(id);
                if (motivo == null)
                {
                    return new JsonResult(new { success = false, message = "Motivo no encontrado", type = "error" });
                }

                if (string.IsNullOrWhiteSpace(descripcion))
                {
                    return new JsonResult(new { success = false, message = "La descripción es requerida", type = "error" });
                }

                // Verificar si ya existe otro motivo con la misma descripción
                var motivoExistente = await _context.Motivo_Recordatorio
                    .AnyAsync(m => m.Descripcion.ToLower() == descripcion.ToLower() && m.Id != id);

                if (motivoExistente)
                {
                    return new JsonResult(new { success = false, message = "Ya existe otro motivo con esa descripción", type = "error" });
                }

                motivo.Descripcion = descripcion.Trim();
                motivo.Estado = estado;

                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Motivo actualizado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al actualizar motivo: {ex.Message}", type = "error" });
            }
        }

        // Handler para eliminar un motivo
        public async Task<IActionResult> OnPostEliminarAsync([FromForm] int id)
        {
            try
            {
                var motivo = await _context.Motivo_Recordatorio.FindAsync(id);
                if (motivo == null)
                {
                    return new JsonResult(new { success = false, message = "Motivo no encontrado", type = "error" });
                }

                // Verificar si el motivo está siendo usado en recordatorios
                var recordatoriosConMotivo = await _context.Recordatorio
                    .AnyAsync(r => r.Motivo_RecordatorioId == id);

                if (recordatoriosConMotivo)
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = "No se puede eliminar el motivo porque tiene recordatorios asociados. ¿Desea desactivarlo en su lugar?", 
                        type = "validation",
                        canDelete = false,
                        shouldDeactivate = true
                    });
                }

                _context.Motivo_Recordatorio.Remove(motivo);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Motivo eliminado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al eliminar motivo: {ex.Message}", type = "error" });
            }
        }

        // Handler para desactivar un motivo
        public async Task<IActionResult> OnPostDesactivarAsync([FromForm] int id)
        {
            try
            {
                var motivo = await _context.Motivo_Recordatorio.FindAsync(id);
                if (motivo == null)
                {
                    return new JsonResult(new { success = false, message = "Motivo no encontrado", type = "error" });
                }

                motivo.Estado = false;
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Motivo desactivado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al desactivar motivo: {ex.Message}", type = "error" });
            }
        }

        // Handler para activar un motivo
        public async Task<IActionResult> OnPostActivarAsync([FromForm] int id)
        {
            try
            {
                var motivo = await _context.Motivo_Recordatorio.FindAsync(id);
                if (motivo == null)
                {
                    return new JsonResult(new { success = false, message = "Motivo no encontrado", type = "error" });
                }

                motivo.Estado = true;
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Motivo activado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al activar motivo: {ex.Message}", type = "error" });
            }
        }
    }
}