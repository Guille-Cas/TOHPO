using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Data;
using Microsoft.EntityFrameworkCore;
using TOHPO.Models;

namespace TOHPO.Pages.Configuracion.Categorias
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public IndexModel(AppDbContext context) { _context = context; }
        public void OnGet() { }

        // Handler para DataTables
        public async Task<JsonResult> OnGetCategoriasAsync()
        {
            try
            {
                var categorias = await _context.Categoria
                    .OrderBy(c => c.Descripcion)
                    .Select(c => new { 
                        id = c.Id, 
                        descripcion = c.Descripcion,
                        estado = c.Estado
                    })
                    .ToListAsync();

                return new JsonResult(categorias);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al cargar categorías: {ex.Message}" });
            }
        }

        // Handler para crear una categoría
        public async Task<IActionResult> OnPostCrearAsync([FromForm] string descripcion)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(descripcion))
                {
                    return new JsonResult(new { success = false, message = "La descripción es requerida", type = "error" });
                }

                // Verificar si ya existe una categoría con la misma descripción
                var categoriaExistente = await _context.Categoria
                    .AnyAsync(c => c.Descripcion.ToLower() == descripcion.ToLower());

                if (categoriaExistente)
                {
                    return new JsonResult(new { success = false, message = "Ya existe una categoría con esa descripción", type = "error" });
                }

                var nuevaCategoria = new Categoria
                {
                    Descripcion = descripcion.Trim(),
                    Estado = true
                };

                _context.Categoria.Add(nuevaCategoria);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Categoría creada exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al crear categoría: {ex.Message}", type = "error" });
            }
        }

        // Handler para editar una categoría
        public async Task<IActionResult> OnPostEditarAsync([FromForm] int id, [FromForm] string descripcion, [FromForm] bool estado)
        {
            try
            {
                var categoria = await _context.Categoria.FindAsync(id);
                if (categoria == null)
                {
                    return new JsonResult(new { success = false, message = "Categoría no encontrada", type = "error" });
                }

                if (string.IsNullOrWhiteSpace(descripcion))
                {
                    return new JsonResult(new { success = false, message = "La descripción es requerida", type = "error" });
                }

                // Verificar si ya existe otra categoría con la misma descripción
                var categoriaExistente = await _context.Categoria
                    .AnyAsync(c => c.Descripcion.ToLower() == descripcion.ToLower() && c.Id != id);

                if (categoriaExistente)
                {
                    return new JsonResult(new { success = false, message = "Ya existe otra categoría con esa descripción", type = "error" });
                }

                categoria.Descripcion = descripcion.Trim();
                categoria.Estado = estado;

                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Categoría actualizada exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al actualizar categoría: {ex.Message}", type = "error" });
            }
        }

        // Handler para eliminar una categoría
        public async Task<IActionResult> OnPostEliminarAsync([FromForm] int id)
        {
            try
            {
                var categoria = await _context.Categoria.FindAsync(id);
                if (categoria == null)
                {
                    return new JsonResult(new { success = false, message = "Categoría no encontrada", type = "error" });
                }

                // Verificar si la categoría está siendo usada en productos
                var productosConCategoria = await _context.Producto
                    .AnyAsync(p => p.Id_Categoria == id);

                if (productosConCategoria)
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = "No se puede eliminar la categoría porque tiene productos asociados. ¿Desea desactivarla en su lugar?", 
                        type = "validation",
                        canDelete = false,
                        shouldDeactivate = true
                    });
                }

                _context.Categoria.Remove(categoria);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Categoría eliminada exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al eliminar categoría: {ex.Message}", type = "error" });
            }
        }

        // Handler para desactivar una categoría
        public async Task<IActionResult> OnPostDesactivarAsync([FromForm] int id)
        {
            try
            {
                var categoria = await _context.Categoria.FindAsync(id);
                if (categoria == null)
                {
                    return new JsonResult(new { success = false, message = "Categoría no encontrada", type = "error" });
                }

                categoria.Estado = false;
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Categoría desactivada exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al desactivar categoría: {ex.Message}", type = "error" });
            }
        }
    }
}
