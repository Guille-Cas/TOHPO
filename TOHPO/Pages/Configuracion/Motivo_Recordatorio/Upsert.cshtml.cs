using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TOHPO.Models;
using TOHPO.Data;

namespace TOHPO.Pages.Configuracion.Motivo_Recordatorio
{
    public class UpsertModel : PageModel
    {
        private readonly AppDbContext _context;

        public UpsertModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public TOHPO.Models.Motivo_Recordatorio MotivoRecordatorio { get; set; } = new TOHPO.Models.Motivo_Recordatorio();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            try
            {
                if (id.HasValue)
                {
                    MotivoRecordatorio = await _context.Motivo_Recordatorio.FindAsync(id.Value);
                    if (MotivoRecordatorio == null)
                    {
                        TempData["ErrorMessage"] = "Motivo no encontrado";
                        return RedirectToPage("./Index");
                    }
                }
                else
                {
                    MotivoRecordatorio = new TOHPO.Models.Motivo_Recordatorio();
                }
                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar el motivo: {ex.Message}";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                // Verificar descripción duplicada
                var motivoExistente = await _context.Motivo_Recordatorio
                    .FirstOrDefaultAsync(m => m.Descripcion.ToLower() == MotivoRecordatorio.Descripcion.ToLower() 
                                           && m.Id != MotivoRecordatorio.Id);

                if (motivoExistente != null)
                {
                    ModelState.AddModelError("MotivoRecordatorio.Descripcion", "Ya existe un motivo con esta descripción");
                    return Page();
                }

                if (MotivoRecordatorio.Id == 0)
                {
                    // Crear nuevo
                    _context.Motivo_Recordatorio.Add(MotivoRecordatorio);
                    TempData["SuccessMessage"] = "Motivo creado exitosamente";
                }
                else
                {
                    // Actualizar existente
                    _context.Motivo_Recordatorio.Update(MotivoRecordatorio);
                    TempData["SuccessMessage"] = "Motivo actualizado exitosamente";
                }

                await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al guardar el motivo: {ex.Message}";
                return Page();
            }
        }
    }
}