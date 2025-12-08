using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Data;
using TOHPO.Models;
using Microsoft.EntityFrameworkCore;

namespace TOHPO.Pages.Configuracion.Impuestos
{
    public class UpsertModel : PageModel
    {
        private readonly AppDbContext _context;
        public UpsertModel(AppDbContext context) { _context = context; }

        [BindProperty]
        public Impuesto Impuesto { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            try
            {
                if (id.HasValue)
                {
                    Impuesto = await _context.Impuesto.FindAsync(id.Value);
                    if (Impuesto == null) 
                    {
                        TempData["error"] = "Impuesto no encontrado";
                        return RedirectToPage("/Configuracion/Impuestos/Index");
                    }
                }
                else
                {
                    Impuesto = new Impuesto { Estado = true }; // Por defecto activo para nuevos impuestos
                }
                return Page();
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error al cargar el impuesto: {ex.Message}";
                return RedirectToPage("/Configuracion/Impuestos/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["error"] = "Por favor corrija los errores en el formulario";
                    return Page();
                }

                if (Impuesto.Porcentaje < 0 || Impuesto.Porcentaje > 100)
                {
                    TempData["error"] = "El porcentaje debe estar entre 0 y 100";
                    return Page();
                }

                // Verificar si ya existe un impuesto con la misma descripción
                var impuestoExistente = await _context.Impuesto
                    .AnyAsync(i => i.Descripcion.ToLower() == Impuesto.Descripcion.ToLower() && i.Id != Impuesto.Id);

                if (impuestoExistente)
                {
                    TempData["error"] = "Ya existe un impuesto con esa descripción";
                    return Page();
                }
                
                if (Impuesto.Id > 0)
                {
                    var existente = await _context.Impuesto.FindAsync(Impuesto.Id);
                    if (existente == null) 
                    {
                        TempData["error"] = "Impuesto no encontrado";
                        return RedirectToPage("/Configuracion/Impuestos/Index");
                    }
                    
                    // Actualizar todas las propiedades
                    existente.Descripcion = Impuesto.Descripcion?.Trim();
                    existente.Porcentaje = Impuesto.Porcentaje;
                    existente.Estado = Impuesto.Estado;
                    
                    _context.Impuesto.Update(existente);
                    TempData["success"] = "Impuesto actualizado exitosamente";
                }
                else
                {
                    Impuesto.Descripcion = Impuesto.Descripcion?.Trim();
                    _context.Impuesto.Add(Impuesto);
                    TempData["success"] = "Impuesto creado exitosamente";
                }
                
                await _context.SaveChangesAsync();
                return RedirectToPage("/Configuracion/Impuestos/Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error al guardar el impuesto: {ex.Message}";
                return Page();
            }
        }
    }
}
