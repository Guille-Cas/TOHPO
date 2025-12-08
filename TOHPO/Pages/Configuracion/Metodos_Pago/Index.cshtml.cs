using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Data;
using Microsoft.EntityFrameworkCore;
using TOHPO.Models;

namespace TOHPO.Pages.Configuracion.Metodos_Pago
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public IndexModel(AppDbContext context) { _context = context; }
        public void OnGet() { }

        // Handler para DataTables
        public async Task<JsonResult> OnGetMetodosPagoAsync()
        {
            try
            {
                var metodos = await _context.Metodo_Pago
                    .OrderBy(m => m.Descripcion)
                    .Select(m => new { 
                        id = m.Id, 
                        descripcion = m.Descripcion,
                        estado = m.Estado
                    })
                    .ToListAsync();

                return new JsonResult(metodos);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al cargar métodos de pago: {ex.Message}" });
            }
        }

        // Handler para crear un método de pago
        public async Task<IActionResult> OnPostCrearAsync([FromForm] string descripcion)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(descripcion))
                {
                    return new JsonResult(new { success = false, message = "La descripción es requerida", type = "error" });
                }

                // Verificar si ya existe un método de pago con la misma descripción
                var metodoExistente = await _context.Metodo_Pago
                    .AnyAsync(m => m.Descripcion.ToLower() == descripcion.ToLower());

                if (metodoExistente)
                {
                    return new JsonResult(new { success = false, message = "Ya existe un método de pago con esa descripción", type = "error" });
                }

                var nuevoMetodo = new Metodo_Pago
                {
                    Descripcion = descripcion.Trim(),
                    Estado = true
                };

                _context.Metodo_Pago.Add(nuevoMetodo);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Método de pago creado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al crear método de pago: {ex.Message}", type = "error" });
            }
        }

        // Handler para editar un método de pago
        public async Task<IActionResult> OnPostEditarAsync([FromForm] int id, [FromForm] string descripcion, [FromForm] bool estado)
        {
            try
            {
                var metodo = await _context.Metodo_Pago.FindAsync(id);
                if (metodo == null)
                {
                    return new JsonResult(new { success = false, message = "Método de pago no encontrado", type = "error" });
                }

                if (string.IsNullOrWhiteSpace(descripcion))
                {
                    return new JsonResult(new { success = false, message = "La descripción es requerida", type = "error" });
                }

                // Verificar si ya existe otro método de pago con la misma descripción
                var metodoExistente = await _context.Metodo_Pago
                    .AnyAsync(m => m.Descripcion.ToLower() == descripcion.ToLower() && m.Id != id);

                if (metodoExistente)
                {
                    return new JsonResult(new { success = false, message = "Ya existe otro método de pago con esa descripción", type = "error" });
                }

                metodo.Descripcion = descripcion.Trim();
                metodo.Estado = estado;

                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Método de pago actualizado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al actualizar método de pago: {ex.Message}", type = "error" });
            }
        }

        // Handler para eliminar un método de pago
        public async Task<IActionResult> OnPostEliminarAsync([FromForm] int id)
        {
            try
            {
                var metodo = await _context.Metodo_Pago.FindAsync(id);
                if (metodo == null)
                {
                    return new JsonResult(new { success = false, message = "Método de pago no encontrado", type = "error" });
                }

                // Verificar si el método de pago está siendo usado en ventas, compras o pedidos
                var ventasConMetodo = await _context.Venta_Metodo_Pago
                    .AnyAsync(vmp => vmp.Id_Metodo_Pago == id);

                var comprasConMetodo = await _context.Compra_Metodo_Pago
                    .AnyAsync(cmp => cmp.Id_Metodo_Pago == id);

                var pedidosConMetodo = await _context.Pedido_Metodo_Pago
                    .AnyAsync(pmp => pmp.Id_Metodo_Pago == id);

                if (ventasConMetodo || comprasConMetodo || pedidosConMetodo)
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = "No se puede eliminar el método de pago porque tiene transacciones asociadas. ¿Desea desactivarlo en su lugar?", 
                        type = "validation",
                        canDelete = false,
                        shouldDeactivate = true
                    });
                }

                _context.Metodo_Pago.Remove(metodo);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Método de pago eliminado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al eliminar método de pago: {ex.Message}", type = "error" });
            }
        }

        // Handler para desactivar un método de pago
        public async Task<IActionResult> OnPostDesactivarAsync([FromForm] int id)
        {
            try
            {
                var metodo = await _context.Metodo_Pago.FindAsync(id);
                if (metodo == null)
                {
                    return new JsonResult(new { success = false, message = "Método de pago no encontrado", type = "error" });
                }

                metodo.Estado = false;
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Método de pago desactivado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al desactivar método de pago: {ex.Message}", type = "error" });
            }
        }
    }
}
