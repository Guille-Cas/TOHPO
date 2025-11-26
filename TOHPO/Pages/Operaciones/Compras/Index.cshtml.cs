using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TOHPO.Data;
using TOHPO.Models;

namespace TOHPO.Pages.Operaciones.Compras
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Compra> Compras { get; set; } = default!;

        [BindProperty]
        public DateTime? FechaInicio { get; set; }

        [BindProperty]
        public DateTime? FechaFin { get; set; }

        [BindProperty]
        public string? BuscarProveedor { get; set; }

        public async Task OnGetAsync()
        {
            await CargarCompras();
        }

        public async Task<IActionResult> OnPostFiltrarAsync()
        {
            await CargarCompras();
            return Page();
        }

        public async Task<IActionResult> OnGetDeleteAsync(int id)
        {
            var compra = await _context.Compra
                .FirstOrDefaultAsync(m => m.Id == id);

            if (compra == null)
            {
                TempData["Error"] = "La compra no fue encontrada.";
                return RedirectToPage("./Index");
            }

            if (compra.Estado)
            {
                TempData["Error"] = "No se puede eliminar una compra que ya ha sido procesada.";
                return RedirectToPage("./Index");
            }

            try
            {
                // Eliminar detalles de compra
                var detalles = await _context.Compra_Detalle
                    .Where(d => d.Id_Compra == id)
                    .ToListAsync();
                _context.Compra_Detalle.RemoveRange(detalles);

                // Eliminar métodos de pago
                var metodosPago = await _context.Compra_Metodo_Pago
                    .Where(mp => mp.Id_Compra == id)
                    .ToListAsync();
                _context.Compra_Metodo_Pago.RemoveRange(metodosPago);

                // Eliminar compra
                _context.Compra.Remove(compra);
                await _context.SaveChangesAsync();

                TempData["Success"] = "La compra ha sido eliminada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar la compra: {ex.Message}";
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetProcesarAsync(int id)
        {
            var compra = await _context.Compra
                .Include(c => c.Compra_Detalles)
                    .ThenInclude(cd => cd.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (compra == null)
            {
                TempData["Error"] = "La compra no fue encontrada.";
                return RedirectToPage("./Index");
            }

            if (compra.Estado)
            {
                TempData["Error"] = "Esta compra ya ha sido procesada.";
                return RedirectToPage("./Index");
            }

            try
            {
                // Validar inventario y actualizar stock
                foreach (var detalle in compra.Compra_Detalles)
                {
                    var inventario = await _context.Inventario
                        .FirstOrDefaultAsync(i => i.Codigo_Producto == detalle.Codigo_Producto);

                    if (inventario == null)
                    {
                        // Crear nuevo registro de inventario si no existe
                        inventario = new TOHPO.Models.Inventario
                        {
                            Codigo_Producto = detalle.Codigo_Producto,
                            Cantidad = detalle.Cantidad,
                            Existencia = detalle.Cantidad,
                            Precio_Compra = detalle.Costo_Unitario,
                            Precio_Venta = detalle.Costo_Unitario * 1.3m, // Margen del 30%
                            Estado = true
                        };
                        _context.Inventario.Add(inventario);
                        await _context.SaveChangesAsync(); // Guardar para obtener el Id
                    }
                    else
                    {
                        // Actualizar inventario existente
                        inventario.Cantidad += detalle.Cantidad;
                        inventario.Existencia += detalle.Cantidad;
                        inventario.Precio_Compra = detalle.Costo_Unitario;
                    }

                    // Crear movimiento de inventario solo si el inventario tiene Id
                    if (inventario.Id > 0)
                    {
                        var movimiento = new Movimiento_Inventario
                        {
                            Id_Inventario = inventario.Id,
                            Cantidad = detalle.Cantidad,
                            Fecha = DateTime.Now,
                            Motivo = $"Compra #{compra.Numero_Factura}",
                        };
                        _context.Movimiento_Inventario.Add(movimiento);
                    }
                }

                // Marcar compra como procesada
                compra.Estado = true;
                await _context.SaveChangesAsync();

                TempData["Success"] = "La compra ha sido procesada correctamente y el inventario ha sido actualizado.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al procesar la compra: {ex.Message}";
            }

            return RedirectToPage("./Index");
        }

        private async Task CargarCompras()
        {
            var query = _context.Compra
                .Include(c => c.Proveedor)
                .AsQueryable();

            // Aplicar filtros
            if (FechaInicio.HasValue)
            {
                query = query.Where(c => c.Fecha >= FechaInicio.Value);
            }

            if (FechaFin.HasValue)
            {
                query = query.Where(c => c.Fecha <= FechaFin.Value);
            }

            if (!string.IsNullOrEmpty(BuscarProveedor))
            {
                query = query.Where(c => c.Proveedor.Nombre.Contains(BuscarProveedor));
            }

            Compras = await query.OrderByDescending(c => c.Fecha).ToListAsync();
        }
    }
}