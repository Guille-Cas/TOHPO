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
            try
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
                                               (p.Cliente.Segundo_Apellido != null && p.Cliente.Segundo_Apellido.Contains(BuscarCliente)));
                    }

                    if (EstadoFiltro.HasValue)
                    {
                        query = query.Where(p => p.Estado == EstadoFiltro.Value);
                    }

                    Pedidos = await query
                        .OrderByDescending(p => p.Fecha_Creacion)
                        .ToListAsync();
                }
                else
                {
                    Pedidos = new List<Pedido>();
                }
            }
            catch (Exception ex)
            {
                // Log del error para debugging
                Console.WriteLine($"Error cargando pedidos: {ex.Message}");
                Pedidos = new List<Pedido>();
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

                // Liberar reservas antes de eliminar
                foreach (var detalle in pedido.Pedido_Detalles)
                {
                    await LiberarReserva(detalle.Codigo_Producto, detalle.Cantidad);
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
                // Si se está completando el pedido
                if (!pedido.Estado)
                {
                    // Validar que hay suficiente inventario para completar
                    var validacionInventario = await ValidarInventarioParaCompletar(pedido);
                    if (!validacionInventario.IsValid)
                    {
                        TempData["ErrorMessage"] = validacionInventario.ErrorMessage;
                        return RedirectToPage();
                    }

                    // COMPLETAR PEDIDO: Liberar reservas y reducir existencia real
                    foreach (var detalle in pedido.Pedido_Detalles)
                    {
                        await CompletarPedidoInventario(detalle.Codigo_Producto, detalle.Cantidad);
                    }
                }
                else
                {
                    // CANCELAR PEDIDO: Devolver existencia y recrear reservas
                    foreach (var detalle in pedido.Pedido_Detalles)
                    {
                        await CancelarPedidoInventario(detalle.Codigo_Producto, detalle.Cantidad);
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

        private async Task CompletarPedidoInventario(string codigoProducto, int cantidad)
        {
            var inventario = await _context.Inventario
                .FirstOrDefaultAsync(i => i.Codigo_Producto == codigoProducto);

            if (inventario != null)
            {
                // Liberar la reserva y reducir la existencia
                inventario.Reservado -= cantidad;
                inventario.Existencia -= cantidad;
                
                // Asegurar que no haya valores negativos
                if (inventario.Reservado < 0) inventario.Reservado = 0;
                if (inventario.Existencia < 0) inventario.Existencia = 0;
                
                _context.Update(inventario);
            }
        }

        private async Task CancelarPedidoInventario(string codigoProducto, int cantidad)
        {
            var inventario = await _context.Inventario
                .FirstOrDefaultAsync(i => i.Codigo_Producto == codigoProducto);

            if (inventario != null)
            {
                // Devolver la existencia y recrear la reserva
                inventario.Existencia += cantidad;
                inventario.Reservado += cantidad;
                _context.Update(inventario);
            }
        }

        private async Task LiberarReserva(string codigoProducto, int cantidad)
        {
            var inventario = await _context.Inventario
                .FirstOrDefaultAsync(i => i.Codigo_Producto == codigoProducto);

            if (inventario != null)
            {
                inventario.Reservado -= cantidad;
                if (inventario.Reservado < 0) inventario.Reservado = 0;
                _context.Update(inventario);
            }
        }

        private async Task<(bool IsValid, string ErrorMessage)> ValidarInventarioParaCompletar(Pedido pedido)
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

                // Verificar que la existencia real sea suficiente
                if (inventario.Existencia < detalle.Cantidad)
                {
                    return (false, $"Stock insuficiente para completar el pedido. {inventario.Producto.Descripcion}: Disponible {inventario.Existencia}, Requerido {detalle.Cantidad}");
                }
            }

            return (true, string.Empty);
        }
    }
}