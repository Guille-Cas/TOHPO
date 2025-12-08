using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Models;
using TOHPO.Data;
using Microsoft.EntityFrameworkCore;

namespace TOHPO.Pages.Configuracion.Presentaciones
{
    public class UpsertModel : PageModel
    {
        private readonly AppDbContext _context;
        public UpsertModel(AppDbContext context) { _context = context; }

        [BindProperty]
        public Presentacion Presentacion { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            try
            {
                if (id.HasValue)
                {
                    Presentacion = await _context.Presentacion.FindAsync(id.Value);
                    if (Presentacion == null) 
                    {
                        TempData["ErrorMessage"] = "Presentación no encontrada";
                        return RedirectToPage("/Configuracion/Presentaciones/Index");
                    }
                }
                else
                {
                    Presentacion = new Presentacion { Estado = true }; // Por defecto activo para nuevas presentaciones
                }
                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar la presentación: {ex.Message}";
                return RedirectToPage("/Configuracion/Presentaciones/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Por favor corrija los errores en el formulario";
                    return Page();
                }

                if (Presentacion.Cantidad <= 0)
                {
                    TempData["ErrorMessage"] = "La cantidad debe ser mayor a cero";
                    return Page();
                }

                // Verificar si ya existe una presentación con la misma cantidad y unidad de medida
                var presentacionExistente = await _context.Presentacion
                    .AnyAsync(p => p.Cantidad == Presentacion.Cantidad && 
                                  p.Unidad_Medida == Presentacion.Unidad_Medida && 
                                  p.Id != Presentacion.Id);

                if (presentacionExistente)
                {
                    TempData["ErrorMessage"] = "Ya existe una presentación con esa cantidad y unidad de medida";
                    return Page();
                }
                
                if (Presentacion.Id > 0)
                {
                    var existente = await _context.Presentacion.FindAsync(Presentacion.Id);
                    if (existente == null) 
                    {
                        TempData["ErrorMessage"] = "Presentación no encontrada";
                        return RedirectToPage("/Configuracion/Presentaciones/Index");
                    }
                    
                    // Actualizar todas las propiedades
                    existente.Cantidad = Presentacion.Cantidad;
                    existente.Unidad_Medida = Presentacion.Unidad_Medida;
                    existente.Estado = Presentacion.Estado;
                    
                    _context.Presentacion.Update(existente);
                    TempData["SuccessMessage"] = "Presentación actualizada exitosamente";
                }
                else
                {
                    _context.Presentacion.Add(Presentacion);
                    TempData["SuccessMessage"] = "Presentación creada exitosamente";
                }
                
                await _context.SaveChangesAsync();
                return RedirectToPage("/Configuracion/Presentaciones/Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al guardar la presentación: {ex.Message}";
                return Page();
            }
        }
    }
}
