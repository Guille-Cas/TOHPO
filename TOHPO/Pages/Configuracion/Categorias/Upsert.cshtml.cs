using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Models;
using TOHPO.Data;
using Microsoft.EntityFrameworkCore;

namespace TOHPO.Pages.Configuracion.Categorias
{
    public class UpsertModel : PageModel
    {
        private readonly AppDbContext _context;
        public UpsertModel(AppDbContext context) { _context = context; }

        [BindProperty]
        public Categoria Categoria { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            try
            {
                if (id.HasValue)
                {
                    Categoria = await _context.Categoria.FindAsync(id.Value);
                    if (Categoria == null) 
                    {
                        TempData["ErrorMessage"] = "Categoría no encontrada";
                        return RedirectToPage("/Configuracion/Categorias/Index");
                    }
                }
                else
                {
                    Categoria = new Categoria { Estado = true }; // Por defecto activo para nuevas categorías
                }
                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar la categoría: {ex.Message}";
                return RedirectToPage("/Configuracion/Categorias/Index");
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

                // Verificar si ya existe una categoría con la misma descripción
                var categoriaExistente = await _context.Categoria
                    .AnyAsync(c => c.Descripcion.ToLower() == Categoria.Descripcion.ToLower() && c.Id != Categoria.Id);

                if (categoriaExistente)
                {
                    TempData["ErrorMessage"] = "Ya existe una categoría con esa descripción";
                    return Page();
                }
                
                if (Categoria.Id > 0)
                {
                    var existente = await _context.Categoria.FindAsync(Categoria.Id);
                    if (existente == null) 
                    {
                        TempData["ErrorMessage"] = "Categoría no encontrada";
                        return RedirectToPage("/Configuracion/Categorias/Index");
                    }
                    
                    // Actualizar ambas propiedades
                    existente.Descripcion = Categoria.Descripcion?.Trim();
                    existente.Estado = Categoria.Estado;
                    
                    _context.Categoria.Update(existente);
                    TempData["SuccessMessage"] = "Categoría actualizada exitosamente";
                }
                else
                {
                    Categoria.Descripcion = Categoria.Descripcion?.Trim();
                    _context.Categoria.Add(Categoria);
                    TempData["SuccessMessage"] = "Categoría creada exitosamente";
                }
                
                await _context.SaveChangesAsync();
                return RedirectToPage("/Configuracion/Categorias/Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al guardar la categoría: {ex.Message}";
                return Page();
            }
        }
    }
}
