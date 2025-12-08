using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TOHPO.Data;
using TOHPO.Models;
using TOHPO.Models.Enums;

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

        [BindProperty]
        public List<DetalleMateriasPrimasViewModel> DetallesMateriasPrimas { get; set; } = new List<DetalleMateriasPrimasViewModel>();

        public SelectList ProductosList { get; set; } = default!;
        public List<Materia_Prima> MateriasPrimasDisponibles { get; set; } = new List<Materia_Prima>();

        public class DetalleMateriasPrimasViewModel
        {
            public int Id { get; set; }
            public int IdMateriaPrima { get; set; }
            public string NombreMateriaPrima { get; set; } = string.Empty;
            public decimal CantidadRequerida { get; set; }
            public Unidad_Medida UnidadMedida { get; set; }
            public string? Observaciones { get; set; }
            public bool Estado { get; set; } = true;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            await CargarDatos();

            if (id.HasValue)
            {
                var receta = await _context.Receta
                    .Include(r => r.Producto)
                    .Include(r => r.Receta_Materias_Primas)
                        .ThenInclude(rmp => rmp.Materia_Prima)
                    .FirstOrDefaultAsync(r => r.Id == id.Value);

                if (receta == null)
                {
                    TempData["ErrorMessage"] = "Receta no encontrada";
                    return RedirectToPage("./Index");
                }

                Receta = receta;

                // Cargar detalles de materias primas para edición
                DetallesMateriasPrimas = receta.Receta_Materias_Primas
                    .Where(rmp => rmp.Materia_Prima != null && rmp.Estado) // Filtrar nulls y solo activos
                    .Select(rmp => new DetalleMateriasPrimasViewModel
                    {
                        Id = rmp.Id,
                        IdMateriaPrima = rmp.Id_Materia_Prima,
                        NombreMateriaPrima = rmp.Materia_Prima.Descripcion,
                        CantidadRequerida = rmp.Cantidad_Requerida,
                        UnidadMedida = rmp.Unidad_Medida,
                        Observaciones = rmp.Observaciones ?? string.Empty,
                        Estado = rmp.Estado
                    }).ToList();

                Console.WriteLine($"Materias primas cargadas para edición: {DetallesMateriasPrimas.Count}");
                foreach (var detalle in DetallesMateriasPrimas)
                {
                    Console.WriteLine($"- {detalle.NombreMateriaPrima}: {detalle.CantidadRequerida} {detalle.UnidadMedida}");
                }
            }
            else
            {
                // Crear nueva receta
                Receta = new Receta
                {
                    Estado = true,
                    Fecha_Creacion = DateTime.Now,
                    Rendimiento = 1
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Remover validaciones de navegación que se cargan por separado
            ModelState.Remove("Receta.Producto");
            ModelState.Remove("Receta.Receta_Materias_Primas");

            // Validar que haya al menos una materia prima
            if (DetallesMateriasPrimas == null || !DetallesMateriasPrimas.Any(d => d.Estado))
            {
                ModelState.AddModelError("", "Debe agregar al menos una materia prima a la receta.");
            }

            // Validar que no haya materias primas duplicadas
            var materiasPrimasActivas = DetallesMateriasPrimas.Where(d => d.Estado).ToList();
            var duplicados = materiasPrimasActivas.GroupBy(d => d.IdMateriaPrima)
                                                 .Where(g => g.Count() > 1)
                                                 .Select(g => g.Key);
            
            if (duplicados.Any())
            {
                ModelState.AddModelError("", "No se pueden agregar materias primas duplicadas.");
            }

            // Validar cantidades requeridas
            foreach (var detalle in materiasPrimasActivas)
            {
                if (detalle.CantidadRequerida <= 0)
                {
                    ModelState.AddModelError("", $"La cantidad requerida para {detalle.NombreMateriaPrima} debe ser mayor a 0.");
                }
            }

            if (!ModelState.IsValid)
            {
                await CargarDatos();
                return Page();
            }

            try
            {
                if (Receta.Id == 0)
                {
                    // Crear nueva receta
                    Receta.Fecha_Creacion = DateTime.Now;
                    _context.Receta.Add(Receta);
                    await _context.SaveChangesAsync();

                    // Agregar detalles de materias primas
                    foreach (var detalle in materiasPrimasActivas)
                    {
                        var recetaMateriaPrima = new Receta_Materia_Prima
                        {
                            Id_Receta = Receta.Id,
                            Id_Materia_Prima = detalle.IdMateriaPrima,
                            Cantidad_Requerida = detalle.CantidadRequerida,
                            Unidad_Medida = detalle.UnidadMedida,
                            Observaciones = detalle.Observaciones,
                            Estado = true
                        };
                        _context.Receta_Materia_Prima.Add(recetaMateriaPrima);
                    }

                    TempData["SuccessMessage"] = "Receta creada exitosamente.";
                }
                else
                {
                    // Actualizar receta existente
                    var recetaExistente = await _context.Receta
                        .Include(r => r.Receta_Materias_Primas)
                        .FirstOrDefaultAsync(r => r.Id == Receta.Id);

                    if (recetaExistente == null)
                    {
                        TempData["ErrorMessage"] = "Receta no encontrada.";
                        return RedirectToPage("./Index");
                    }

                    // Actualizar datos principales
                    recetaExistente.Descripcion = Receta.Descripcion;
                    recetaExistente.Codigo_Producto = Receta.Codigo_Producto;
                    recetaExistente.Rendimiento = Receta.Rendimiento;
                    recetaExistente.Instrucciones = Receta.Instrucciones;
                    recetaExistente.Detalle = Receta.Detalle;
                    recetaExistente.Cantidad_Empaque = Receta.Cantidad_Empaque;
                    recetaExistente.Estado = Receta.Estado;

                    // Eliminar detalles existentes
                    _context.Receta_Materia_Prima.RemoveRange(recetaExistente.Receta_Materias_Primas);

                    // Agregar nuevos detalles
                    foreach (var detalle in materiasPrimasActivas)
                    {
                        var recetaMateriaPrima = new Receta_Materia_Prima
                        {
                            Id_Receta = recetaExistente.Id,
                            Id_Materia_Prima = detalle.IdMateriaPrima,
                            Cantidad_Requerida = detalle.CantidadRequerida,
                            Unidad_Medida = detalle.UnidadMedida,
                            Observaciones = detalle.Observaciones,
                            Estado = true
                        };
                        _context.Receta_Materia_Prima.Add(recetaMateriaPrima);
                    }

                    TempData["SuccessMessage"] = "Receta actualizada exitosamente.";
                }

                await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }
            catch (DbUpdateException dbEx)
            {
                ModelState.AddModelError("", $"Error de base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}");
                await CargarDatos();
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error inesperado: {ex.Message}");
                await CargarDatos();
                return Page();
            }
        }

        public async Task<IActionResult> OnGetMateriasPrimasAsync()
        {
            try
            {
                var materiasPrimas = await _context.Materia_Prima
                    .Where(mp => mp.Estado)
                    .OrderBy(mp => mp.Descripcion)
                    .Select(mp => new
                    {
                        id = mp.Id,
                        descripcion = mp.Descripcion,
                        unidadMedida = mp.Unidad_Medida.ToString()
                    })
                    .AsNoTracking()
                    .ToListAsync();

                return new JsonResult(new { success = true, materiasPrimas });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        private async Task CargarDatos()
        {
            try
            {
                // Cargar solo productos que NO son materias primas (productos terminados)
                var productos = await _context.Producto
                    .Where(p => p.Estado && !p.Es_Materia_Prima)
                    .OrderBy(p => p.Descripcion)
                    .AsNoTracking()
                    .ToListAsync();
                ProductosList = new SelectList(productos, "CodigoReferencia", "Descripcion", Receta?.Codigo_Producto);

                // Cargar materias primas activas
                MateriasPrimasDisponibles = await _context.Materia_Prima
                    .Where(mp => mp.Estado)
                    .OrderBy(mp => mp.Descripcion)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en CargarDatos: {ex.Message}");
            }
        }
    }
}