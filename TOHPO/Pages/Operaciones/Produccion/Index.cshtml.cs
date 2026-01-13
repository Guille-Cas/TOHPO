using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TOHPO.Data;
using TOHPO.Models;

namespace TOHPO.Pages.Operaciones.Produccion
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Receta> Recetas { get; set; } = default!;
        public IList<Models.Produccion> Producciones { get; set; } = default!;

        [BindProperty]
        public string? BuscarReceta { get; set; }

        [BindProperty]
        public DateTime? FechaInicio { get; set; }

        [BindProperty]
        public DateTime? FechaFin { get; set; }

        [BindProperty]
        public int RecetaIdProduccion { get; set; }

        [BindProperty]
        public double CantidadProducir { get; set; }

        [BindProperty]
        public string? ObraProduccion { get; set; }

        [BindProperty]
        public string? DescripcionProduccion { get; set; }

        [BindProperty]
        public DateTime FechaPlaneada { get; set; } = DateTime.Today.AddDays(1);

        public async Task OnGetAsync()
        {
            await CargarDatos();
        }

        public async Task<IActionResult> OnPostFiltrarAsync()
        {
            await CargarDatos();
            return Page();
        }

        private async Task CargarDatos()
        {
            // Cargar recetas con productos
            var queryRecetas = _context.Receta
                .Include(r => r.Producto)
                .Where(r => r.Estado)
                .AsQueryable();

            if (!string.IsNullOrEmpty(BuscarReceta))
            {
                queryRecetas = queryRecetas.Where(r => 
                    r.Descripcion.Contains(BuscarReceta) ||
                    r.Producto.Descripcion.Contains(BuscarReceta));
            }

            Recetas = await queryRecetas
                .OrderBy(r => r.Descripcion)
                .ToListAsync();

            // Cargar producciones con detalles y recetas
            var queryProducciones = _context.Produccion
                .Include(p => p.Produccion_Detalles)
                    .ThenInclude(pd => pd.Receta)
                .Include(p => p.Produccion_Detalles)
                    .ThenInclude(pd => pd.Producto)
                .AsQueryable();

            if (FechaInicio.HasValue)
            {
                queryProducciones = queryProducciones.Where(p => p.Fecha >= FechaInicio.Value);
            }

            if (FechaFin.HasValue)
            {
                queryProducciones = queryProducciones.Where(p => p.Fecha <= FechaFin.Value);
            }

            Producciones = await queryProducciones
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();
        }

        public async Task<IActionResult> OnGetEliminarRecetaAsync(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "ID de receta no válido";
                return RedirectToPage();
            }

            var receta = await _context.Receta.FindAsync(id);
            if (receta == null)
            {
                TempData["ErrorMessage"] = "Receta no encontrada";
                return RedirectToPage();
            }

            try
            {
                // Verificar si la receta está siendo usada en producciones activas
                var produccionesActivas = await _context.Produccion_Detalle
                    .Where(pd => pd.Id_Receta == id)
                    .Include(pd => pd.Produccion)
                    .AnyAsync(pd => pd.Produccion.Estado);

                if (produccionesActivas)
                {
                    TempData["ErrorMessage"] = "No se puede eliminar la receta porque está siendo utilizada en producciones activas";
                    return RedirectToPage();
                }

                _context.Receta.Remove(receta);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Receta eliminada exitosamente";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar la receta: " + ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostGenerarProduccionAsync()
        {
            if (RecetaIdProduccion <= 0 || CantidadProducir <= 0)
            {
                TempData["ErrorMessage"] = "Debe seleccionar una receta y especificar una cantidad válida";
                await CargarDatos();
                return Page();
            }

            var receta = await _context.Receta
                .Include(r => r.Producto)
                .Include(r => r.Receta_Materias_Primas)
                    .ThenInclude(rmp => rmp.Materia_Prima)
                .FirstOrDefaultAsync(r => r.Id == RecetaIdProduccion);

            if (receta == null)
            {
                TempData["ErrorMessage"] = "Receta no encontrada";
                await CargarDatos();
                return Page();
            }

            try
            {
                // Validar inventario disponible
                var validacionInventario = await ValidarInventarioParaProduccion(receta, CantidadProducir);
                if (!validacionInventario.EsValido)
                {
                    TempData["ErrorMessage"] = "Inventario insuficiente: " + validacionInventario.Mensaje;
                    await CargarDatos();
                    return Page();
                }

                // Crear la producción
                var produccion = new Models.Produccion
                {
                    Fecha = DateTime.Now,
                    Obra = ObraProduccion ?? "",
                    Descripcion = DescripcionProduccion ?? $"Producción de {receta.Producto.Descripcion}",
                    Fecha_Planeada = FechaPlaneada,
                    Estado = true
                };

                _context.Produccion.Add(produccion);
                await _context.SaveChangesAsync();

                // Crear el detalle de producción
                var detalle = new Produccion_Detalle
                {
                    Id_Produccion = produccion.Id,
                    Id_Receta = receta.Id,
                    Codigo_Producto = receta.Codigo_Producto,
                    Cantidad_Programada = CantidadProducir,
                    Cantidad_Producida = 0,
                    Estado = true,
                    Fecha_Inicio = DateTime.Now
                };

                _context.Produccion_Detalle.Add(detalle);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Producción generada exitosamente. ID: {produccion.Id}";
                
                // Limpiar formulario
                RecetaIdProduccion = 0;
                CantidadProducir = 0;
                ObraProduccion = "";
                DescripcionProduccion = "";
                FechaPlaneada = DateTime.Today.AddDays(1);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al generar la producción: " + ex.Message;
            }

            await CargarDatos();
            return Page();
        }

        // NUEVO: Método para finalizar la producción
        public async Task<IActionResult> OnGetFinalizarProduccionAsync(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "ID de producción no válido";
                return RedirectToPage();
            }

            try
            {
                var produccion = await _context.Produccion
                    .Include(p => p.Produccion_Detalles)
                        .ThenInclude(pd => pd.Receta)
                            .ThenInclude(r => r.Receta_Materias_Primas)
                                .ThenInclude(rmp => rmp.Materia_Prima)
                    .Include(p => p.Produccion_Detalles)
                        .ThenInclude(pd => pd.Producto)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (produccion == null)
                {
                    TempData["ErrorMessage"] = "Producción no encontrada";
                    return RedirectToPage();
                }

                if (!produccion.Estado)
                {
                    TempData["ErrorMessage"] = "La producción ya está finalizada";
                    return RedirectToPage();
                }

                // Validar que hay detalles de producción
                if (!produccion.Produccion_Detalles.Any())
                {
                    TempData["ErrorMessage"] = "No se encontraron detalles de producción";
                    return RedirectToPage();
                }

                // Finalizar cada detalle de producción
                var resultadoFinalizacion = await FinalizarProduccionCompleta(produccion);
                
                if (resultadoFinalizacion.Exitoso)
                {
                    TempData["SuccessMessage"] = $"Producción finalizada exitosamente. {resultadoFinalizacion.Mensaje}";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Error al finalizar la producción: {resultadoFinalizacion.Mensaje}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al finalizar la producción: " + ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetCambiarEstadoProduccionAsync(int id)
        {
            var produccion = await _context.Produccion.FindAsync(id);
            if (produccion == null)
            {
                TempData["ErrorMessage"] = "Producción no encontrada";
                return RedirectToPage();
            }

            try
            {
                produccion.Estado = !produccion.Estado;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Estado de producción {(produccion.Estado ? "activado" : "desactivado")} exitosamente";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cambiar el estado: " + ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<JsonResult> OnGetDetalleRecetaAsync(int id)
        {
            var receta = await _context.Receta
                .Include(r => r.Producto)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (receta == null)
            {
                return new JsonResult(new { success = false, message = "Receta no encontrada" });
            }

            var detalle = new
            {
                success = true,
                receta = new
                {
                    id = receta.Id,
                    descripcion = receta.Descripcion,
                    rendimiento = receta.Rendimiento,
                    instrucciones = receta.Instrucciones,
                    detalle = receta.Detalle ?? "",
                    cantidadEmpaque = receta.Cantidad_Empaque,
                    producto = receta.Producto?.Descripcion ?? "Sin producto",
                    fechaCreacion = receta.Fecha_Creacion.ToString("dd/MM/yyyy")
                }
            };

            return new JsonResult(detalle);
        }

        public async Task<JsonResult> OnGetDetalleProduccionAsync(int id)
        {
            var produccion = await _context.Produccion
                .Include(p => p.Produccion_Detalles)
                    .ThenInclude(pd => pd.Receta)
                .Include(p => p.Produccion_Detalles)
                    .ThenInclude(pd => pd.Producto)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produccion == null)
            {
                return new JsonResult(new { success = false, message = "Producción no encontrada" });
            }

            var detalle = new
            {
                success = true,
                produccion = new
                {
                    id = produccion.Id,
                    fecha = produccion.Fecha.ToString("dd/MM/yyyy HH:mm"),
                    obra = produccion.Obra,
                    descripcion = produccion.Descripcion,
                    fechaPlaneada = produccion.Fecha_Planeada.ToString("dd/MM/yyyy"),
                    estado = produccion.Estado,
                    detalles = produccion.Produccion_Detalles.Select(pd => new
                    {
                        id = pd.Id,
                        receta = pd.Receta?.Descripcion ?? "Sin receta",
                        producto = pd.Producto?.Descripcion ?? "Sin producto",
                        cantidadProgramada = pd.Cantidad_Programada,
                        cantidadProducida = pd.Cantidad_Producida,
                        fechaInicio = pd.Fecha_Inicio?.ToString("dd/MM/yyyy HH:mm"),
                        fechaFin = pd.Fecha_Fin?.ToString("dd/MM/yyyy HH:mm"),
                        observaciones = pd.Observaciones,
                        estado = pd.Estado
                    }).ToList()
                }
            };

            return new JsonResult(detalle);
        }

        private async Task<(bool EsValido, string Mensaje)> ValidarInventarioParaProduccion(Receta receta, double cantidad)
        {
            try
            {
                var mensajesError = new List<string>();

                // Validar materias primas
                foreach (var materiaPrimaReceta in receta.Receta_Materias_Primas.Where(rmp => rmp.Estado))
                {
                    // Buscar si la materia prima está registrada como producto en inventario
                    var inventarioMateriaPrima = await _context.Inventario
                        .Include(i => i.Producto)
                        .FirstOrDefaultAsync(i => i.Producto.Es_Materia_Prima && 
                                                 i.Producto.Id_Materia_Prima == materiaPrimaReceta.Id_Materia_Prima);

                    if (inventarioMateriaPrima == null)
                    {
                        mensajesError.Add($"La materia prima '{materiaPrimaReceta.Materia_Prima?.Descripcion}' no está registrada en inventario");
                        continue;
                    }

                    var cantidadNecesaria = (double)materiaPrimaReceta.Cantidad_Requerida * cantidad;
                    
                    if (inventarioMateriaPrima.Existencia < cantidadNecesaria)
                    {
                        mensajesError.Add($"Insuficiente '{materiaPrimaReceta.Materia_Prima?.Descripcion}': Disponible {inventarioMateriaPrima.Existencia}, Necesario {cantidadNecesaria:N3}");
                    }
                }

                if (mensajesError.Any())
                {
                    return (false, string.Join("; ", mensajesError));
                }

                return (true, "Inventario suficiente para la producción");
            }
            catch (Exception ex)
            {
                return (false, "Error al validar inventario: " + ex.Message);
            }
        }

        // NUEVO: Método principal para finalizar la producción completa
        private async Task<(bool Exitoso, string Mensaje)> FinalizarProduccionCompleta(Models.Produccion produccion)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var resumenMovimientos = new List<string>();

                foreach (var detalle in produccion.Produccion_Detalles)
                {
                    // Finalizar el detalle de producción
                    detalle.Cantidad_Producida = detalle.Cantidad_Programada; // Asumir que se produce la cantidad programada
                    detalle.Fecha_Fin = DateTime.Now;
                    detalle.Estado = false; // Marcar como terminado

                    // Procesar movimientos de inventario para este detalle
                    var resultadoMovimientos = await ProcesarMovimientosInventario(detalle);
                    
                    if (!resultadoMovimientos.Exitoso)
                    {
                        await transaction.RollbackAsync();
                        return (false, resultadoMovimientos.Mensaje);
                    }

                    resumenMovimientos.Add(resultadoMovimientos.Mensaje);
                }

                // Finalizar la producción
                produccion.Estado = false; // Marcar como finalizada
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var mensajeFinal = $"Producción completada. Movimientos: {string.Join("; ", resumenMovimientos)}";
                return (true, mensajeFinal);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, ex.Message);
            }
        }

        // NUEVO: Procesar los movimientos de inventario para un detalle de producción
        private async Task<(bool Exitoso, string Mensaje)> ProcesarMovimientosInventario(Produccion_Detalle detalle)
        {
            try
            {
                var movimientos = new List<string>();

                // 1. Descontar materias primas del inventario
                var receta = await _context.Receta
                    .Include(r => r.Receta_Materias_Primas)
                        .ThenInclude(rmp => rmp.Materia_Prima)
                    .FirstOrDefaultAsync(r => r.Id == detalle.Id_Receta);

                if (receta == null)
                {
                    return (false, "Receta no encontrada");
                }

                foreach (var materiaPrimaReceta in receta.Receta_Materias_Primas.Where(rmp => rmp.Estado))
                {
                    var resultado = await DescontarMateriaPrimaInventario(
                        materiaPrimaReceta, 
                        detalle.Cantidad_Producida, 
                        detalle.Id);

                    if (!resultado.Exitoso)
                    {
                        return resultado;
                    }

                    movimientos.Add(resultado.Mensaje);
                }

                // 2. Agregar producto terminado al inventario
                var resultadoProducto = await AgregarProductoTerminadoInventario(detalle);
                
                if (!resultadoProducto.Exitoso)
                {
                    return resultadoProducto;
                }

                movimientos.Add(resultadoProducto.Mensaje);

                return (true, string.Join(", ", movimientos));
            }
            catch (Exception ex)
            {
                return (false, $"Error procesando movimientos: {ex.Message}");
            }
        }

        // NUEVO: Descontar materia prima del inventario
        private async Task<(bool Exitoso, string Mensaje)> DescontarMateriaPrimaInventario(
            Receta_Materia_Prima materiaPrimaReceta, 
            double cantidadProducida, 
            int detalleProduccionId)
        {
            try
            {
                // Buscar el inventario de la materia prima
                var inventarioMateriaPrima = await _context.Inventario
                    .Include(i => i.Producto)
                    .FirstOrDefaultAsync(i => i.Producto.Es_Materia_Prima && 
                                             i.Producto.Id_Materia_Prima == materiaPrimaReceta.Id_Materia_Prima);

                if (inventarioMateriaPrima == null)
                {
                    return (false, $"Inventario no encontrado para materia prima: {materiaPrimaReceta.Materia_Prima?.Descripcion}");
                }

                var cantidadADescontar = (int)Math.Ceiling((double)materiaPrimaReceta.Cantidad_Requerida * cantidadProducida);

                if (inventarioMateriaPrima.Existencia < cantidadADescontar)
                {
                    return (false, $"Inventario insuficiente para {materiaPrimaReceta.Materia_Prima?.Descripcion}: Disponible {inventarioMateriaPrima.Existencia}, Necesario {cantidadADescontar}");
                }

                // Descontar del inventario
                inventarioMateriaPrima.Existencia -= cantidadADescontar;

                // Registrar movimiento de inventario
                var movimiento = new Movimiento_Inventario
                {
                    Id_Inventario = inventarioMateriaPrima.Id,
                    Cantidad = -cantidadADescontar, // Cantidad negativa para descuento
                    Motivo = $"Producción #{detalleProduccionId} - Consumo de materia prima",
                    Fecha = DateTime.Now
                };

                _context.Movimiento_Inventario.Add(movimiento);

                return (true, $"Descontado {cantidadADescontar} de {materiaPrimaReceta.Materia_Prima?.Descripcion}");
            }
            catch (Exception ex)
            {
                return (false, $"Error descontando materia prima: {ex.Message}");
            }
        }

        // NUEVO: Agregar producto terminado al inventario
        private async Task<(bool Exitoso, string Mensaje)> AgregarProductoTerminadoInventario(Produccion_Detalle detalle)
        {
            try
            {
                // Buscar o crear inventario para el producto terminado
                var inventarioProducto = await _context.Inventario
                    .FirstOrDefaultAsync(i => i.Codigo_Producto == detalle.Codigo_Producto);

                if (inventarioProducto == null)
                {
                    // Crear inventario para el producto si no existe
                    var producto = await _context.Producto.FirstOrDefaultAsync(p => p.CodigoReferencia == detalle.Codigo_Producto);
                    
                    if (producto == null)
                    {
                        return (false, $"Producto no encontrado: {detalle.Codigo_Producto}");
                    }

                    inventarioProducto = new TOHPO.Models.Inventario
                    {
                        Codigo_Producto = detalle.Codigo_Producto,
                        Cantidad = 0,
                        Existencia = 0,
                        Precio_Venta = 0,
                        Precio_Compra = 0,
                        Estado = true
                    };

                    _context.Inventario.Add(inventarioProducto);
                    await _context.SaveChangesAsync(); // Guardar para obtener el ID
                }

                var cantidadAgregar = (int)Math.Floor(detalle.Cantidad_Producida);

                // Agregar al inventario
                inventarioProducto.Existencia += cantidadAgregar;
                inventarioProducto.Cantidad += cantidadAgregar;

                // Registrar movimiento de inventario
                var movimiento = new Movimiento_Inventario
                {
                    Id_Inventario = inventarioProducto.Id,
                    Cantidad = cantidadAgregar, // Cantidad positiva para ingreso
                    Motivo = $"Producción #{detalle.Id} - Producto terminado",
                    Fecha = DateTime.Now
                };

                _context.Movimiento_Inventario.Add(movimiento);

                return (true, $"Agregado {cantidadAgregar} unidades de producto terminado");
            }
            catch (Exception ex)
            {
                return (false, $"Error agregando producto terminado: {ex.Message}");
            }
        }
    }
}