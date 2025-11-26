using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TOHPO.Data;
using TOHPO.Models;

namespace TOHPO.Pages.Operaciones.Inventario
{
    public class UpsertModel : PageModel
    {
        private readonly AppDbContext _context;

        public UpsertModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Models.Inventario Inventario { get; set; } = default!;

        public SelectList ProductosList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            await LoadProductosAsync();

            if (id == null)
            {
                // Crear nuevo inventario
                Inventario = new Models.Inventario
                {
                    Estado = true,
                    Cantidad = 0
                };
                return Page();
            }

            var inventario = await _context.Inventario
                .Include(i => i.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (inventario == null)
            {
                return NotFound();
            }

            Inventario = inventario;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Inventario.Producto");
            if (!ModelState.IsValid)
            {
                await LoadProductosAsync();
                return Page();
            }

            try
            {
                if (Inventario.Id == 0)
                {
                    // Verificar si ya existe un registro para este producto
                    var existingInventario = await _context.Inventario
                        .FirstOrDefaultAsync(i => i.Codigo_Producto == Inventario.Codigo_Producto);

                    if (existingInventario != null)
                    {
                        // Actualizar el existente (Upsert)
                        existingInventario.Cantidad = Inventario.Cantidad;
                        existingInventario.Estado = Inventario.Estado;
                        _context.Inventario.Update(existingInventario);
                    }
                    else
                    {
                        // Crear nuevo
                        _context.Inventario.Add(Inventario);
                    }
                }
                else
                {
                    // Actualizar existente
                    _context.Inventario.Update(Inventario);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Inventario guardado exitosamente.";
                return RedirectToPage("./Index");
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Error al guardar los cambios: " + ex.Message);
                await LoadProductosAsync();
                return Page();
            }
        }

        private async Task LoadProductosAsync()
        {
            var productos = await _context.Producto
                .Where(p => p.Estado == true)
                .OrderBy(p => p.Descripcion)
                .Select(p => new { p.CodigoReferencia, p.Descripcion })
                .ToListAsync();

            ProductosList = new SelectList(productos, "CodigoReferencia", "Descripcion", Inventario?.Codigo_Producto);
        }
    }
}