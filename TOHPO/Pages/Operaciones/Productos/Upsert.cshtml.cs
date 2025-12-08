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
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    // Crear nuevo producto
                    Producto = new Producto
                    {
                        Estado = true,
                        Es_Materia_Prima = false,
                        Es_De_Terceros = false,
                        Se_Daña = false,
                        Tiempo_De_Vida = 0,
                        Unidad_Medida = Unidad_Medida.Unidad
                    };
                }
                else
                {
                    var producto = await _context.Producto
                        .Include(p => p.Categoria)
                        .Include(p => p.Impuesto)
                        .Include(p => p.Materia_Prima)
                        .Include(p => p.Presentacion)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(m => m.CodigoReferencia == id);

                    if (producto == null)
                    {
                        TempData["ErrorMessage"] = "Producto no encontrado";
                        return RedirectToPage("./Index");
                    }

                    Producto = producto;
                }
                
                // Cargar las listas DESPUÉS de asignar el producto
                await LoadSelectListsAsync();
                
                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en OnGetAsync: {ex.Message}");
                TempData["ErrorMessage"] = $"Error al cargar el producto: {ex.Message}";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Remover validaciones de navegación que se cargan por separado
            ModelState.Remove("Producto.Categoria");
            ModelState.Remove("Producto.Impuesto");
            ModelState.Remove("Producto.Materia_Prima");
            ModelState.Remove("Producto.Presentacion");
            ModelState.Remove("Producto.Inventario");

            // Validaciones condicionales
            if (Producto.Es_Materia_Prima && (!Producto.Id_Materia_Prima.HasValue || Producto.Id_Materia_Prima.Value == 0))
            {
                ModelState.AddModelError("Producto.Id_Materia_Prima", "Debe seleccionar una materia prima cuando el producto es materia prima.");
            }

            if (Producto.Se_Daña && Producto.Tiempo_De_Vida <= 0)
            {
                ModelState.AddModelError("Producto.Tiempo_De_Vida", "Debe especificar un tiempo de vida mayor a 0 cuando el producto se daña con el tiempo.");
            }

            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                return Page();
            }

            try
            {
                // Lógica condicional para campos opcionales
                if (!Producto.Es_Materia_Prima)
                {
                    Producto.Id_Materia_Prima = null;
                }

                if (!Producto.Se_Daña)
                {
                    Producto.Tiempo_De_Vida = 0;
                }

                // Verificar si es un producto existente
                var existingProduct = await _context.Producto
                    .FirstOrDefaultAsync(p => p.CodigoReferencia == Producto.CodigoReferencia);

                if (existingProduct != null)
                {
                    // Actualizar producto existente
                    existingProduct.Descripcion = Producto.Descripcion;
                    existingProduct.Id_Categoria = Producto.Id_Categoria;
                    existingProduct.Id_Impuesto = Producto.Id_Impuesto;
                    existingProduct.Id_Materia_Prima = Producto.Id_Materia_Prima;
                    existingProduct.Id_Presentacion = Producto.Id_Presentacion;
                    existingProduct.Es_Materia_Prima = Producto.Es_Materia_Prima;
                    existingProduct.Es_De_Terceros = Producto.Es_De_Terceros;
                    existingProduct.Se_Daña = Producto.Se_Daña;
                    existingProduct.Tiempo_De_Vida = Producto.Tiempo_De_Vida;
                    existingProduct.Unidad_Medida = Producto.Unidad_Medida;
                    existingProduct.Estado = Producto.Estado;

                    _context.Producto.Update(existingProduct);
                    TempData["SuccessMessage"] = "Producto actualizado exitosamente.";
                }
                else
                {
                    // Crear nuevo producto
                    _context.Producto.Add(Producto);
                    TempData["SuccessMessage"] = "Producto creado exitosamente.";
                }

                await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }
            catch (DbUpdateException dbEx)
            {
                ModelState.AddModelError("", $"Error de base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}");
                await LoadSelectListsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error inesperado: {ex.Message}");
                await LoadSelectListsAsync();
                return Page();
            }
        }

        private async Task LoadSelectListsAsync()
        {
            try
            {
                // Cargar categorías activas
                var categorias = await _context.Categoria
                    .Where(c => c.Estado)
                    .OrderBy(c => c.Descripcion)
                    .AsNoTracking()
                    .ToListAsync();
                CategoriasList = new SelectList(categorias, "Id", "Descripcion", Producto?.Id_Categoria);

                // Cargar impuestos
                var impuestos = await _context.Impuesto
                    .OrderBy(i => i.Descripcion)
                    .AsNoTracking()
                    .ToListAsync();
                ImpuestosList = new SelectList(impuestos, "Id", "Descripcion", Producto?.Id_Impuesto);

                // Cargar materias primas activas
                var materiasPrimas = await _context.Materia_Prima
                    .Where(mp => mp.Estado)
                    .OrderBy(mp => mp.Descripcion)
                    .AsNoTracking()
                    .ToListAsync();
                MateriaPrimasList = new SelectList(materiasPrimas, "Id", "Descripcion", Producto?.Id_Materia_Prima);

                // Cargar presentaciones con descripción completa
                var presentaciones = await _context.Presentacion
                    .OrderBy(p => p.Cantidad)
                    .AsNoTracking()
                    .Select(p => new { 
                        p.Id, 
                        Descripcion = $"{p.Cantidad} {p.Unidad_Medida}" 
                    })
                    .ToListAsync();
                PresentacionesList = new SelectList(presentaciones, "Id", "Descripcion", Producto?.Id_Presentacion);
            }
            catch (Exception ex)
            {
                // Log error pero continúa con listas vacías
                CategoriasList = new SelectList(new List<object>(), "Id", "Descripcion");
                ImpuestosList = new SelectList(new List<object>(), "Id", "Descripcion");
                MateriaPrimasList = new SelectList(new List<object>(), "Id", "Descripcion");
                PresentacionesList = new SelectList(new List<object>(), "Id", "Descripcion");
            }
        }
    }
}