using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Data;
using Microsoft.EntityFrameworkCore;
using TOHPO.Models;
using TOHPO.Models.Enums;

namespace TOHPO.Pages.Configuracion.Materias_Prima
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public IndexModel(AppDbContext context) { _context = context; }
        public void OnGet() { }

        // Handler para DataTables
        public async Task<JsonResult> OnGetMateriasPrimaAsync()
        {
            try
            {
                var materias = await _context.Materia_Prima
                    .OrderBy(m => m.Descripcion)
                    .Select(m => new { 
                        id = m.Id, 
                        descripcion = m.Descripcion, 
                        unidad_Medida = m.Unidad_Medida.ToString(),
                        estado = m.Estado
                    })
                    .ToListAsync();

                return new JsonResult(materias);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al cargar materias primas: {ex.Message}" });
            }
        }

        // Handler para crear una materia prima
        public async Task<IActionResult> OnPostCrearAsync([FromForm] string descripcion, [FromForm] Unidad_Medida unidad_Medida)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(descripcion))
                {
                    return new JsonResult(new { success = false, message = "La descripción es requerida", type = "error" });
                }

                // Verificar si ya existe una materia prima con la misma descripción
                var materiaExistente = await _context.Materia_Prima
                    .AnyAsync(m => m.Descripcion.ToLower() == descripcion.ToLower());

                if (materiaExistente)
                {
                    return new JsonResult(new { success = false, message = "Ya existe una materia prima con esa descripción", type = "error" });
                }

                var nuevaMateria = new Materia_Prima
                {
                    Descripcion = descripcion.Trim(),
                    Unidad_Medida = unidad_Medida,
                    Estado = true
                };

                _context.Materia_Prima.Add(nuevaMateria);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Materia prima creada exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al crear materia prima: {ex.Message}", type = "error" });
            }
        }

        // Handler para editar una materia prima
        public async Task<IActionResult> OnPostEditarAsync([FromForm] int id, [FromForm] string descripcion, [FromForm] Unidad_Medida unidad_Medida, [FromForm] bool estado)
        {
            try
            {
                var materia = await _context.Materia_Prima.FindAsync(id);
                if (materia == null)
                {
                    return new JsonResult(new { success = false, message = "Materia prima no encontrada", type = "error" });
                }

                if (string.IsNullOrWhiteSpace(descripcion))
                {
                    return new JsonResult(new { success = false, message = "La descripción es requerida", type = "error" });
                }

                // Verificar si ya existe otra materia prima con la misma descripción
                var materiaExistente = await _context.Materia_Prima
                    .AnyAsync(m => m.Descripcion.ToLower() == descripcion.ToLower() && m.Id != id);

                if (materiaExistente)
                {
                    return new JsonResult(new { success = false, message = "Ya existe otra materia prima con esa descripción", type = "error" });
                }

                materia.Descripcion = descripcion.Trim();
                materia.Unidad_Medida = unidad_Medida;
                materia.Estado = estado;

                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Materia prima actualizada exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al actualizar materia prima: {ex.Message}", type = "error" });
            }
        }

        // Handler para eliminar una materia prima
        public async Task<IActionResult> OnPostEliminarAsync([FromForm] int id)
        {
            try
            {
                var materia = await _context.Materia_Prima.FindAsync(id);
                if (materia == null)
                {
                    return new JsonResult(new { success = false, message = "Materia prima no encontrada", type = "error" });
                }

                // Verificar si la materia prima está siendo usada en recetas
                var recetasConMateria = await _context.Receta_Materia_Prima
                    .AnyAsync(r => r.Id_Materia_Prima == id);

                // Verificar si está en productos (para materias primas que son productos)
                var productosConMateria = await _context.Producto
                    .AnyAsync(p => p.Id_Materia_Prima == id);

                if (recetasConMateria || productosConMateria)
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = "No se puede eliminar la materia prima porque tiene recetas o productos asociados. ¿Desea desactivarla en su lugar?", 
                        type = "validation",
                        canDelete = false,
                        shouldDeactivate = true
                    });
                }

                _context.Materia_Prima.Remove(materia);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Materia prima eliminada exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al eliminar materia prima: {ex.Message}", type = "error" });
            }
        }

        // Handler para desactivar una materia prima
        public async Task<IActionResult> OnPostDesactivarAsync([FromForm] int id)
        {
            try
            {
                var materia = await _context.Materia_Prima.FindAsync(id);
                if (materia == null)
                {
                    return new JsonResult(new { success = false, message = "Materia prima no encontrada", type = "error" });
                }

                materia.Estado = false;
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Materia prima desactivada exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al desactivar materia prima: {ex.Message}", type = "error" });
            }
        }
    }
}
