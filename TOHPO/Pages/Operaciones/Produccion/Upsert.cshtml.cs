using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TOHPO.Data;
using TOHPO.Models;
using System.Text.Json;
using TOHPO.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace TOHPO.Pages.Operaciones.Produccion
{
    public class UpsertModel : PageModel
    {
        private readonly AppDbContext _context;

        public UpsertModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Receta Receta { get; set; } = default!;

        public SelectList ProductosSelectList { get; set; } = default!;
        public SelectList MateriasPrimasSelectList { get; set; } = default!;

        [BindProperty]
        public List<RecetaMateriaPrimaDto> MateriasPrimasSeleccionadas { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            await CargarSelectLists();

            if (id == null)
            {
                // Nueva receta
                Receta = new Receta
                {
                    Fecha_Creacion = DateTime.Now,
                    Estado = true,
                    Rendimiento = 1.0,
                    Cantidad_Empaque = 1.0
                };
                return Page();
            }

            // Cargar receta existente
            Receta = await _context.Receta
                .Include(r => r.Producto)
                    .ThenInclude(p => p.Categoria)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (Receta == null)
            {
                return NotFound();
            }

            // CORREGIDO: Cargar las materias primas con mejor manejo de la relación
            var recetasMateriasPrimas = await _context.Receta_Materia_Prima
                .Include(rmp => rmp.Materia_Prima)
                .Where(rmp => rmp.Id_Receta == id && rmp.Estado)
                .ToListAsync();

            // CORREGIDO: Mapeo más robusto con validaciones adicionales
            MateriasPrimasSeleccionadas = new List<RecetaMateriaPrimaDto>();
            
            foreach (var rmp in recetasMateriasPrimas)
            {
                // Verificar que la materia prima no sea nula
                if (rmp.Materia_Prima != null)
                {
                    var dto = new RecetaMateriaPrimaDto
                    {
                        Id = rmp.Id,
                        Id_Materia_Prima = rmp.Id_Materia_Prima,
                        Descripcion = rmp.Materia_Prima.Descripcion ?? "Sin descripción",
                        Cantidad_Requerida = rmp.Cantidad_Requerida,
                        Unidad_Medida = rmp.Unidad_Medida,
                        Observaciones = rmp.Observaciones ?? ""
                    };
                    
                    MateriasPrimasSeleccionadas.Add(dto);
                    
                    // DEBUG: Log para verificar qué se está agregando
                    Console.WriteLine($"Agregando materia prima: {dto.Descripcion}, Cantidad: {dto.Cantidad_Requerida}");
                }
                else
                {
                    Console.WriteLine($"Materia prima nula para ID: {rmp.Id_Materia_Prima}");
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Remover validaciones que no son necesarias
            ModelState.Remove("Receta.Producto");
            
            // Remover validaciones de Observaciones para todos los elementos
            var observacionesKeys = ModelState.Keys
                .Where(k => k.Contains("MateriasPrimasSeleccionadas") && k.EndsWith(".Observaciones"))
                .ToList();
            
            foreach (var key in observacionesKeys)
            {
                ModelState.Remove(key);
            }

            // CORREGIDO: Remover validaciones de Descripción ya que es solo informativa
            var descripcionKeys = ModelState.Keys
                .Where(k => k.Contains("MateriasPrimasSeleccionadas") && k.EndsWith(".Descripcion"))
                .ToList();
            
            foreach (var key in descripcionKeys)
            {
                ModelState.Remove(key);
            }

            if (!ModelState.IsValid)
            {
                await CargarSelectLists();
                return Page();
            }

            if (MateriasPrimasSeleccionadas == null || !MateriasPrimasSeleccionadas.Any())
            {
                ModelState.AddModelError("", "Debe agregar al menos una materia prima a la receta");
                await CargarSelectLists();
                return Page();
            }

            try
            {
                // Verificar que el producto existe
                var producto = await _context.Producto
                    .FirstOrDefaultAsync(p => p.CodigoReferencia == Receta.Codigo_Producto);
                
                if (producto == null)
                {
                    ModelState.AddModelError("Receta.Codigo_Producto", "El producto seleccionado no existe");
                    await CargarSelectLists();
                    return Page();
                }

                if (Receta.Id == 0)
                {
                    // Nueva receta
                    Receta.Fecha_Creacion = DateTime.Now;
                    _context.Receta.Add(Receta);
                    await _context.SaveChangesAsync();

                    // Agregar materias primas
                    await GuardarMateriasPrimas(Receta.Id);
                    
                    TempData["SuccessMessage"] = "Receta creada exitosamente";
                }
                else
                {
                    // Editar receta existente
                    var recetaExistente = await _context.Receta.FindAsync(Receta.Id);
                    
                    if (recetaExistente == null)
                    {
                        return NotFound();
                    }

                    // Verificar si la receta está siendo usada en producciones activas
                    var produccionesActivas = await _context.Produccion_Detalle
                        .Where(pd => pd.Id_Receta == Receta.Id)
                        .Include(pd => pd.Produccion)
                        .AnyAsync(pd => pd.Produccion.Estado);

                    if (produccionesActivas && (
                        recetaExistente.Codigo_Producto != Receta.Codigo_Producto ||
                        recetaExistente.Rendimiento != Receta.Rendimiento))
                    {
                        ModelState.AddModelError("", "No se pueden modificar el producto o rendimiento de una receta que está siendo utilizada en producciones activas");
                        await CargarSelectLists();
                        return Page();
                    }

                    // Actualizar campos de la receta
                    recetaExistente.Descripcion = Receta.Descripcion;
                    recetaExistente.Codigo_Producto = Receta.Codigo_Producto;
                    recetaExistente.Rendimiento = Receta.Rendimiento;
                    recetaExistente.Instrucciones = Receta.Instrucciones;
                    recetaExistente.Detalle = Receta.Detalle;
                    recetaExistente.Cantidad_Empaque = Receta.Cantidad_Empaque;
                    recetaExistente.Estado = Receta.Estado;

                    // CORREGIDO: Actualizar materias primas de forma más eficiente
                    var materiasPrimasExistentes = await _context.Receta_Materia_Prima
                        .Where(rmp => rmp.Id_Receta == Receta.Id)
                        .ToListAsync();

                    // Eliminar las existentes
                    _context.Receta_Materia_Prima.RemoveRange(materiasPrimasExistentes);
                    await _context.SaveChangesAsync();

                    // Agregar las nuevas
                    await GuardarMateriasPrimas(Receta.Id);

                    TempData["SuccessMessage"] = "Receta actualizada exitosamente";
                }

                await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al guardar la receta: " + ex.Message);
                await CargarSelectLists();
                return Page();
            }
        }

        private async Task GuardarMateriasPrimas(int recetaId)
        {
            foreach (var mp in MateriasPrimasSeleccionadas)
            {
                // CORREGIDO: Verificar que la materia prima existe
                var materiaPrimaExiste = await _context.Materia_Prima
                    .AnyAsync(m => m.Id == mp.Id_Materia_Prima && m.Estado);

                if (!materiaPrimaExiste)
                {
                    continue; // Saltar si la materia prima no existe o está inactiva
                }

                var recetaMateriaPrima = new Receta_Materia_Prima
                {
                    Id_Receta = recetaId,
                    Id_Materia_Prima = mp.Id_Materia_Prima,
                    Cantidad_Requerida = mp.Cantidad_Requerida,
                    Unidad_Medida = mp.Unidad_Medida,
                    Observaciones = string.IsNullOrWhiteSpace(mp.Observaciones) ? null : mp.Observaciones.Trim(),
                    Estado = true
                };

                _context.Receta_Materia_Prima.Add(recetaMateriaPrima);
            }
        }

        private async Task CargarSelectLists()
        {
            var productos = await _context.Producto
                .Where(p => p.Estado)
                .OrderBy(p => p.Descripcion)
                .ToListAsync();

            ProductosSelectList = new SelectList(productos, "CodigoReferencia", "Descripcion");

            var materiasPrimas = await _context.Materia_Prima
                .Where(mp => mp.Estado)
                .OrderBy(mp => mp.Descripcion)
                .ToListAsync();

            MateriasPrimasSelectList = new SelectList(materiasPrimas, "Id", "Descripcion");
        }

        // CORREGIDO: Mejorar la validación del producto para evitar la advertencia en edición
        public async Task<JsonResult> OnGetValidarProductoAsync(string codigo)
        {
            var producto = await _context.Producto
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.CodigoReferencia == codigo && p.Estado);

            if (producto == null)
            {
                return new JsonResult(new { valido = false, mensaje = "Producto no encontrado o inactivo" });
            }

            // CORREGIDO: Obtener el ID de la receta actual desde la URL o parámetros
            var currentRecetaId = 0;
            if (Request.Query.TryGetValue("id", out var idValue) && int.TryParse(idValue, out var parsedId))
            {
                currentRecetaId = parsedId;
            }

            // Verificar si ya existe una receta para este producto (excluyendo la receta actual)
            var recetaExistente = await _context.Receta
                .FirstOrDefaultAsync(r => r.Codigo_Producto == codigo && r.Id != currentRecetaId);

            if (recetaExistente != null)
            {
                return new JsonResult(new 
                { 
                    valido = false, 
                    mensaje = "Ya existe una receta para este producto",
                    recetaId = recetaExistente.Id,
                    recetaDescripcion = recetaExistente.Descripcion
                });
            }

            return new JsonResult(new 
            { 
                valido = true, 
                producto = new 
                {
                    codigo = producto.CodigoReferencia,
                    descripcion = producto.Descripcion,
                    categoria = producto.Categoria?.Descripcion ?? "Sin categoría"
                }
            });
        }

        public async Task<JsonResult> OnGetObtenerMateriaPrimaAsync(int id)
        {
            var materiaPrima = await _context.Materia_Prima.FindAsync(id);
            
            if (materiaPrima == null)
            {
                return new JsonResult(new { success = false, mensaje = "Materia prima no encontrada" });
            }

            return new JsonResult(new 
            { 
                success = true,
                materia_prima = new 
                {
                    id = materiaPrima.Id,
                    descripcion = materiaPrima.Descripcion,
                    unidad_medida = materiaPrima.Unidad_Medida
                }
            });
        }
    }

    // DTO para manejar las materias primas en el formulario
    public class RecetaMateriaPrimaDto
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Debe seleccionar una materia prima")]
        public int Id_Materia_Prima { get; set; }
        
        public string Descripcion { get; set; } = "";
        
        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(0.001, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal Cantidad_Requerida { get; set; }
        
        [Required(ErrorMessage = "Debe seleccionar una unidad de medida")]
        public Unidad_Medida Unidad_Medida { get; set; }
        
        // Las observaciones son opcionales
        public string? Observaciones { get; set; }
    }
}