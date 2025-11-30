using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TOHPO.Data;
using TOHPO.Models;

namespace TOHPO.Pages.Operaciones.Pedidos
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Pedido> Pedidos { get; set; } = default!;

        [BindProperty]
        public DateTime? FechaInicio { get; set; }

        [BindProperty]
        public DateTime? FechaFin { get; set; }

        [BindProperty]
        public string? BuscarCliente { get; set; }

        [BindProperty]
        public bool? EstadoFiltro { get; set; }

        public async Task OnGetAsync()
        {
            await CargarPedidos();
        }

        public async Task<IActionResult> OnPostFiltrarAsync()
        {
            await CargarPedidos();
            return Page();
        }

        private async Task CargarPedidos()
        {
            if (_context.Pedido != null)
            {
                var query = _context.Pedido
                    .Include(p => p.Cliente)
                    .Include(p => p.Pedido_Detalles)
                        .ThenInclude(pd => pd.Producto)
                    .AsQueryable();

                // Filtros
                if (FechaInicio.HasValue)
                {
                    query = query.Where(p => p.Fecha_Creacion >= FechaInicio.Value);
                }

                if (FechaFin.HasValue)
                {
                    query = query.Where(p => p.Fecha_Entrega <= FechaFin.Value);
                }

                if (!string.IsNullOrEmpty(BuscarCliente))
                {
                    query = query.Where(p => p.Cliente.Nombre.Contains(BuscarCliente) ||
                                           p.Cliente.Primer_Apellido.Contains(BuscarCliente) ||
                                           p.Cliente.Segundo_Apellido.Contains(BuscarCliente));
                }

                if (EstadoFiltro.HasValue)
                {
                    query = query.Where(p => p.Estado == EstadoFiltro.Value);
                }

                Pedidos = await query
                    .OrderByDescending(p => p.Fecha_Creacion)
                    .ToListAsync();
            }
        }

        public async Task<IActionResult> OnGetEliminarAsync(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "ID de pedido no válido";
                return RedirectToPage();
            }

            var pedido = await _context.Pedido
                .Include(p => p.Pedido_Detalles)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
            {
                TempData["ErrorMessage"] = "Pedido no encontrado";
                return RedirectToPage();
            }

            try
            {
                // Verificar que el pedido no esté completado para poder eliminarlo
                if (pedido.Estado)
                {
                    TempData["ErrorMessage"] = "No se puede eliminar un pedido completado";
                    return RedirectToPage();
                }

                // Devolver productos al inventario antes de eliminar
                foreach (var detalle in pedido.Pedido_Detalles)
                {
                    var inventario = await _context.Inventario
                        .FirstOrDefaultAsync(i => i.Codigo_Producto == detalle.Codigo_Producto);

                    if (inventario != null)
                    {
                        inventario.Existencia += detalle.Cantidad;
                        _context.Update(inventario);
                    }
                }

                // Eliminar detalles del pedido
                if (pedido.Pedido_Detalles.Any())
                {
                    _context.Pedido_Detalle.RemoveRange(pedido.Pedido_Detalles);
                }

                // Eliminar el pedido
                _context.Pedido.Remove(pedido);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Pedido eliminado correctamente";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al eliminar el pedido: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetToggleEstadoAsync(int id)
        {
            var pedido = await _context.Pedido
                .Include(p => p.Pedido_Detalles)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
            {
                TempData["ErrorMessage"] = "Pedido no encontrado";
                return RedirectToPage();
            }

            try
            {
                // Si se está completando el pedido, verificar inventario
                if (!pedido.Estado)
                {
                    var validacionInventario = await ValidarInventarioParaPedido(pedido);
                    if (!validacionInventario.IsValid)
                    {
                        TempData["ErrorMessage"] = validacionInventario.ErrorMessage;
                        return RedirectToPage();
                    }

                    // Reducir inventario al completar pedido
                    foreach (var detalle in pedido.Pedido_Detalles)
                    {
                        var inventario = await _context.Inventario
                            .FirstOrDefaultAsync(i => i.Codigo_Producto == detalle.Codigo_Producto);

                        if (inventario != null)
                        {
                            inventario.Existencia -= detalle.Cantidad;
                            _context.Update(inventario);
                        }
                    }
                }
                else
                {
                    // Si se está cancelando el pedido, devolver productos al inventario
                    foreach (var detalle in pedido.Pedido_Detalles)
                    {
                        var inventario = await _context.Inventario
                            .FirstOrDefaultAsync(i => i.Codigo_Producto == detalle.Codigo_Producto);

                        if (inventario != null)
                        {
                            inventario.Existencia += detalle.Cantidad;
                            _context.Update(inventario);
                        }
                    }
                }

                pedido.Estado = !pedido.Estado;
                _context.Update(pedido);
                await _context.SaveChangesAsync();

                string mensaje = pedido.Estado ? "Pedido completado correctamente" : "Pedido cancelado correctamente";
                TempData["SuccessMessage"] = mensaje;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cambiar el estado del pedido: {ex.Message}";
            }

            return RedirectToPage();
        }

        private async Task<(bool IsValid, string ErrorMessage)> ValidarInventarioParaPedido(Pedido pedido)
        {
            foreach (var detalle in pedido.Pedido_Detalles)
            {
                var inventario = await _context.Inventario
                    .Include(i => i.Producto)
                    .FirstOrDefaultAsync(i => i.Codigo_Producto == detalle.Codigo_Producto);

                if (inventario == null)
                {
                    return (false, $"Producto {detalle.Codigo_Producto} no encontrado en inventario");
                }

                if (inventario.Existencia < detalle.Cantidad)
                {
                    return (false, $"Stock insuficiente para {inventario.Producto.Descripcion}. Disponible: {inventario.Existencia}, Requerido: {detalle.Cantidad}");
                }
            }

            return (true, string.Empty);
        }
    }
}