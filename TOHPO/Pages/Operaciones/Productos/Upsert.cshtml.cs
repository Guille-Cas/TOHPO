using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TOHPO.Data;
using TOHPO.Models;
using TOHPO.Models.Enums;

namespace TOHPO.Pages.Operaciones.Productos
{
    public class UpsertModel : PageModel
    {
        private readonly AppDbContext _context;

        public UpsertModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Producto Producto { get; set; } = default!;

        public SelectList CategoriasList { get; set; } = default!;
        public SelectList ImpuestosList { get; set; } = default!;
        public SelectList MateriaPrimasList { get; set; } = default!;
        public SelectList PresentacionesList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            await LoadSelectListsAsync();

            if (string.IsNullOrEmpty(id))
            {
                // Crear nuevo producto
                Producto = new Producto
                {
                    Estado = true,
                    Es_Materia_Prima = false,
                    Es_De_Terceros = false,
                    Tiempo_De_Vida = 0,
                    Unidad_Medida = Unidad_Medida.Unidad
                };
                return Page();
            }

            var producto = await _context.Producto
                .Include(p => p.Categoria)
                .Include(p => p.Impuesto)
                .Include(p => p.Materia_Prima)
                .Include(p => p.Presentacion)
                .FirstOrDefaultAsync(m => m.CodigoReferencia == id);

            if (producto == null)
            {
                return NotFound();
            }

            Producto = producto;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Remover validaciones de navegación que se cargan por separado
            ModelState.Remove("Producto.Categoria");
            ModelState.Remove("Producto.Impuesto");
            ModelState.Remove("Producto.Materia_Prima");
            ModelState.Remove("Producto.Presentacion");

            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                return Page();
            }

            try
            {
                var isNew = string.IsNullOrEmpty(Producto.CodigoReferencia) || 
                           !await _context.Producto.AnyAsync(p => p.CodigoReferencia == Producto.CodigoReferencia);

                if (isNew)
                {
                    // Verificar que el código no exista
                    var existingProduct = await _context.Producto
                        .FirstOrDefaultAsync(p => p.CodigoReferencia == Producto.CodigoReferencia);

                    if (existingProduct != null)
                    {
                        ModelState.AddModelError("Producto.CodigoReferencia", 
                            "Ya existe un producto con este código de referencia.");
                        await LoadSelectListsAsync();
                        return Page();
                    }

                    _context.Producto.Add(Producto);
                    TempData["SuccessMessage"] = "Producto creado exitosamente.";
                }
                else
                {
                    _context.Producto.Update(Producto);
                    TempData["SuccessMessage"] = "Producto actualizado exitosamente.";
                }

                await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Error al guardar los cambios: " + ex.Message);
                await LoadSelectListsAsync();
                return Page();
            }
        }

        private async Task LoadSelectListsAsync()
        {
            // Cargar categorías
            var categorias = await _context.Categoria
                .Where(c => c.Estado == true)
                .OrderBy(c => c.Descripcion)
                .ToListAsync();
            CategoriasList = new SelectList(categorias, "Id", "Descripcion", Producto?.Id_Categoria);

            // Cargar impuestos
            var impuestos = await _context.Impuesto
                .Where(i => i.Estado == true)
                .OrderBy(i => i.Descripcion)
                .ToListAsync();
            ImpuestosList = new SelectList(impuestos, "Id", "Descripcion", Producto?.Id_Impuesto);

            // Cargar materias primas
            var materiasPrimas = await _context.Materia_Prima
                .Where(mp => mp.Estado == true)
                .OrderBy(mp => mp.Descripcion)
                .ToListAsync();
            MateriaPrimasList = new SelectList(materiasPrimas, "Id", "Descripcion", Producto?.Id_Materia_Prima);

            // Cargar presentaciones
            var presentaciones = await _context.Presentacion
                .Where(p => p.Estado == true)
                .OrderBy(p => p.Cantidad)
                .ToListAsync();
            PresentacionesList = new SelectList(presentaciones, "Id", "Cantidad", Producto?.Id_Presentacion);
        }
    }
}