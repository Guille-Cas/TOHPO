using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Models;
using TOHPO.Data;
using Microsoft.EntityFrameworkCore;

namespace TOHPO.Pages.Configuracion.Materias_Prima
{
    public class UpsertModel : PageModel
    {
        private readonly AppDbContext _context;
        public UpsertModel(AppDbContext context) { _context = context; }

        [BindProperty]
        public Materia_Prima MateriaPrima { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            try
            {
                if (id.HasValue)
                {
                    MateriaPrima = await _context.Materia_Prima.FindAsync(id.Value);
                    if (MateriaPrima == null) 
                    {
                        TempData["error"] = "Materia prima no encontrada";
                        return RedirectToPage("/Configuracion/Materias_Prima/Index");
                    }
                }
                else
                {
                    MateriaPrima = new Materia_Prima { Estado = true }; // Por defecto activo para nuevas materias primas
                }
                return Page();
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error al cargar la materia prima: {ex.Message}";
                return RedirectToPage("/Configuracion/Materias_Prima/Index");
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

                // Verificar si ya existe una materia prima con la misma descripción
                var materiaExistente = await _context.Materia_Prima
                    .AnyAsync(m => m.Descripcion.ToLower() == MateriaPrima.Descripcion.ToLower() && m.Id != MateriaPrima.Id);

                if (materiaExistente)
                {
                    TempData["error"] = "Ya existe una materia prima con esa descripción";
                    return Page();
                }
                
                if (MateriaPrima.Id > 0)
                {
                    var existente = await _context.Materia_Prima.FindAsync(MateriaPrima.Id);
                    if (existente == null) 
                    {
                        TempData["error"] = "Materia prima no encontrada";
                        return RedirectToPage("/Configuracion/Materias_Prima/Index");
                    }
                    
                    // Actualizar todas las propiedades
                    existente.Descripcion = MateriaPrima.Descripcion?.Trim();
                    existente.Unidad_Medida = MateriaPrima.Unidad_Medida;
                    existente.Estado = MateriaPrima.Estado;

                    _context.Materia_Prima.Update(existente);
                    TempData["success"] = "Materia prima actualizada exitosamente";
                }
                else
                {
                    MateriaPrima.Descripcion = MateriaPrima.Descripcion?.Trim();
                    _context.Materia_Prima.Add(MateriaPrima);
                    TempData["success"] = "Materia prima creada exitosamente";
                }
                
                await _context.SaveChangesAsync();
                return RedirectToPage("/Configuracion/Materias_Prima/Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error al guardar la materia prima: {ex.Message}";
                return Page();
            }
        }
    }
}
