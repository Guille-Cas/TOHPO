using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Data;
using Microsoft.EntityFrameworkCore;
using TOHPO.Models;

namespace TOHPO.Pages.Configuracion.Impuestos
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public IndexModel(AppDbContext context) { _context = context; }
        public void OnGet() { }

        // Handler para DataTables
        public async Task<JsonResult> OnGetImpuestosAsync()
        {
            try
            {
                var impuestos = await _context.Impuesto
                    .OrderBy(i => i.Descripcion)
                    .Select(i => new { 
                        id = i.Id, 
                        descripcion = i.Descripcion, 
                        porcentaje = i.Porcentaje,
                        estado = i.Estado
                    })
                    .ToListAsync();

                return new JsonResult(impuestos);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al cargar impuestos: {ex.Message}" });
            }
        }

        // Handler para crear un impuesto
        public async Task<IActionResult> OnPostCrearAsync([FromForm] string descripcion, [FromForm] decimal porcentaje)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(descripcion))
                {
                    return new JsonResult(new { success = false, message = "La descripción es requerida", type = "error" });
                }

                if (porcentaje < 0 || porcentaje > 100)
                {
                    return new JsonResult(new { success = false, message = "El porcentaje debe estar entre 0 y 100", type = "error" });
                }

                // Verificar si ya existe un impuesto con la misma descripción
                var impuestoExistente = await _context.Impuesto
                    .AnyAsync(i => i.Descripcion.ToLower() == descripcion.ToLower());

                if (impuestoExistente)
                {
                    return new JsonResult(new { success = false, message = "Ya existe un impuesto con esa descripción", type = "error" });
                }

                var nuevoImpuesto = new Impuesto
                {
                    Descripcion = descripcion.Trim(),
                    Porcentaje = porcentaje,
                    Estado = true
                };

                _context.Impuesto.Add(nuevoImpuesto);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Impuesto creado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al crear impuesto: {ex.Message}", type = "error" });
            }
        }

        // Handler para editar un impuesto
        public async Task<IActionResult> OnPostEditarAsync([FromForm] int id, [FromForm] string descripcion, [FromForm] decimal porcentaje, [FromForm] bool estado)
        {
            try
            {
                var impuesto = await _context.Impuesto.FindAsync(id);
                if (impuesto == null)
                {
                    return new JsonResult(new { success = false, message = "Impuesto no encontrado", type = "error" });
                }

                if (string.IsNullOrWhiteSpace(descripcion))
                {
                    return new JsonResult(new { success = false, message = "La descripción es requerida", type = "error" });
                }

                if (porcentaje < 0 || porcentaje > 100)
                {
                    return new JsonResult(new { success = false, message = "El porcentaje debe estar entre 0 y 100", type = "error" });
                }

                // Verificar si ya existe otro impuesto con la misma descripción
                var impuestoExistente = await _context.Impuesto
                    .AnyAsync(i => i.Descripcion.ToLower() == descripcion.ToLower() && i.Id != id);

                if (impuestoExistente)
                {
                    return new JsonResult(new { success = false, message = "Ya existe otro impuesto con esa descripción", type = "error" });
                }

                impuesto.Descripcion = descripcion.Trim();
                impuesto.Porcentaje = porcentaje;
                impuesto.Estado = estado;

                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Impuesto actualizado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al actualizar impuesto: {ex.Message}", type = "error" });
            }
        }

        // Handler para eliminar un impuesto
        public async Task<IActionResult> OnPostEliminarAsync([FromForm] int id)
        {
            try
            {
                var impuesto = await _context.Impuesto.FindAsync(id);
                if (impuesto == null)
                {
                    return new JsonResult(new { success = false, message = "Impuesto no encontrado", type = "error" });
                }

                // Verificar si el impuesto está siendo usado en productos
                var productosConImpuesto = await _context.Producto
                    .AnyAsync(p => p.Id_Impuesto == id);

                if (productosConImpuesto)
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = "No se puede eliminar el impuesto porque tiene productos asociados. ¿Desea desactivarlo en su lugar?", 
                        type = "validation",
                        canDelete = false,
                        shouldDeactivate = true
                    });
                }

                _context.Impuesto.Remove(impuesto);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Impuesto eliminado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al eliminar impuesto: {ex.Message}", type = "error" });
            }
        }

        // Handler para desactivar un impuesto
        public async Task<IActionResult> OnPostDesactivarAsync([FromForm] int id)
        {
            try
            {
                var impuesto = await _context.Impuesto.FindAsync(id);
                if (impuesto == null)
                {
                    return new JsonResult(new { success = false, message = "Impuesto no encontrado", type = "error" });
                }

                impuesto.Estado = false;
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Impuesto desactivado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al desactivar impuesto: {ex.Message}", type = "error" });
            }
        }
    }
}
