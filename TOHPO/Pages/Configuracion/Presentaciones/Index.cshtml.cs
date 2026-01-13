using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Data;
using Microsoft.EntityFrameworkCore;
using TOHPO.Models;
using TOHPO.Models.Enums;

namespace TOHPO.Pages.Configuracion.Presentaciones
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public IndexModel(AppDbContext context) { _context = context; }
        public void OnGet() { }

        // Handler para DataTables
        public async Task<JsonResult> OnGetPresentacionesAsync()
        {
            try
            {
                var presentaciones = await _context.Presentacion
                    .OrderBy(p => p.Cantidad)
                    .ThenBy(p => p.Unidad_Medida)
                    .Select(p => new { 
                        id = p.Id, 
                        cantidad = p.Cantidad, 
                        unidad_Medida = p.Unidad_Medida.ToString(),
                        estado = p.Estado
                    })
                    .ToListAsync();

                return new JsonResult(presentaciones);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al cargar presentaciones: {ex.Message}" });
            }
        }

        // Handler para crear una presentación
        public async Task<IActionResult> OnPostCrearAsync([FromForm] double cantidad, [FromForm] Unidad_Medida unidad_Medida)
        {
            try
            {
                if (cantidad <= 0)
                {
                    return new JsonResult(new { success = false, message = "La cantidad debe ser mayor a cero", type = "error" });
                }

                // Verificar si ya existe una presentación con la misma cantidad y unidad de medida
                var presentacionExistente = await _context.Presentacion
                    .AnyAsync(p => p.Cantidad == cantidad && p.Unidad_Medida == unidad_Medida);

                if (presentacionExistente)
                {
                    return new JsonResult(new { success = false, message = "Ya existe una presentación con esa cantidad y unidad de medida", type = "error" });
                }

                var nuevaPresentacion = new Presentacion
                {
                    Cantidad = cantidad,
                    Unidad_Medida = unidad_Medida,
                    Estado = true
                };

                _context.Presentacion.Add(nuevaPresentacion);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Presentación creada exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al crear presentación: {ex.Message}", type = "error" });
            }
        }

        // Handler para editar una presentación
        public async Task<IActionResult> OnPostEditarAsync([FromForm] int id, [FromForm] double cantidad, [FromForm] Unidad_Medida unidad_Medida, [FromForm] bool estado)
        {
            try
            {
                var presentacion = await _context.Presentacion.FindAsync(id);
                if (presentacion == null)
                {
                    return new JsonResult(new { success = false, message = "Presentación no encontrada", type = "error" });
                }

                if (cantidad <= 0)
                {
                    return new JsonResult(new { success = false, message = "La cantidad debe ser mayor a cero", type = "error" });
                }

                // Verificar si ya existe otra presentación con la misma cantidad y unidad de medida
                var presentacionExistente = await _context.Presentacion
                    .AnyAsync(p => p.Cantidad == cantidad && p.Unidad_Medida == unidad_Medida && p.Id != id);

                if (presentacionExistente)
                {
                    return new JsonResult(new { success = false, message = "Ya existe otra presentación con esa cantidad y unidad de medida", type = "error" });
                }

                presentacion.Cantidad = cantidad;
                presentacion.Unidad_Medida = unidad_Medida;
                presentacion.Estado = estado;

                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Presentación actualizada exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al actualizar presentación: {ex.Message}", type = "error" });
            }
        }

        // Handler para eliminar una presentación
        public async Task<IActionResult> OnPostEliminarAsync([FromForm] int id)
        {
            try
            {
                var presentacion = await _context.Presentacion.FindAsync(id);
                if (presentacion == null)
                {
                    return new JsonResult(new { success = false, message = "Presentación no encontrada", type = "error" });
                }

                // Verificar si la presentación está siendo usada en productos
                var productosConPresentacion = await _context.Producto
                    .AnyAsync(p => p.Id_Presentacion == id);

                if (productosConPresentacion)
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = "No se puede eliminar la presentación porque tiene productos asociados. ¿Desea desactivarla en su lugar?", 
                        type = "validation",
                        canDelete = false,
                        shouldDeactivate = true
                    });
                }

                _context.Presentacion.Remove(presentacion);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Presentación eliminada exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al eliminar presentación: {ex.Message}", type = "error" });
            }
        }

        // Handler para desactivar una presentación
        public async Task<IActionResult> OnPostDesactivarAsync([FromForm] int id)
        {
            try
            {
                var presentacion = await _context.Presentacion.FindAsync(id);
                if (presentacion == null)
                {
                    return new JsonResult(new { success = false, message = "Presentación no encontrada", type = "error" });
                }

                presentacion.Estado = false;
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Presentación desactivada exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al desactivar presentación: {ex.Message}", type = "error" });
            }
        }
    }
}
