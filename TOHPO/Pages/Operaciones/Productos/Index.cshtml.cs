using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TOHPO.Data;
using TOHPO.Models;

namespace TOHPO.Pages.Operaciones.Productos
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Producto> Productos { get; set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.Producto != null)
            {
                Productos = await _context.Producto
                    .Include(p => p.Categoria)
                    .Include(p => p.Impuesto)
                    .Include(p => p.Materia_Prima)
                    .Include(p => p.Presentacion)
                    .OrderBy(p => p.Descripcion)
                    .ToListAsync();
            }
        }

        public async Task<IActionResult> OnGetEliminarAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID de producto no válido";
                return RedirectToPage();
            }

            var producto = await _context.Producto.FindAsync(id);
            if (producto == null)
            {
                TempData["ErrorMessage"] = "Producto no encontrado";
                return RedirectToPage();
            }

            try
            {
                // Verificar si el producto está siendo usado en otras tablas
                var enUsoEnInventario = await _context.Inventario
                    .AnyAsync(i => i.Codigo_Producto == id);
                
                var enUsoEnVentas = await _context.Detalle_Venta
                    .AnyAsync(dv => dv.Codigo_Producto == id);

                if (enUsoEnInventario || enUsoEnVentas)
                {
                    // En lugar de eliminar, desactivar el producto
                    producto.Estado = false;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Producto desactivado exitosamente (estaba en uso)";
                }
                else
                {
                    // Eliminar completamente si no está en uso
                    _context.Producto.Remove(producto);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Producto eliminado exitosamente";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar el producto: " + ex.Message;
            }

            return RedirectToPage();
        }
    }
}