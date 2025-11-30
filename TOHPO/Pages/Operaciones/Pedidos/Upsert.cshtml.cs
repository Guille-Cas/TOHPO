using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TOHPO.Data;
using TOHPO.Models;
using System.ComponentModel.DataAnnotations;

namespace TOHPO.Pages.Operaciones.Pedidos
{
    public class UpsertModel : PageModel
    {
        private readonly AppDbContext _context;

        public UpsertModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Pedido Pedido { get; set; } = default!;

        [BindProperty]
        public List<PedidoDetalleDto> DetallesPedido { get; set; } = new List<PedidoDetalleDto>();

        public SelectList ClientesList { get; set; } = default!;
        public List<ProductoInventarioDto> ProductosDisponibles { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            await LoadSelectListsAsync();

            if (id == null)
            {
                // Crear nuevo pedido
                Pedido = new Pedido
                {
                    Fecha_Creacion = DateTime.Now,
                    Fecha_Entrega = DateTime.Now.AddDays(7),
                    Estado = false,
                    Abono = 0,
                    Saldo = 0,
                    Total = 0,
                };
                return Page();
            }

           var pedido = await _context.Pedido
                .Include(p => p.Cliente)
                .Include(p => p.Pedido_Detalles)
                    .ThenInclude(pd => pd.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pedido == null)
            {
                return NotFound();
            }

            Pedido = pedido;

            // Cargar detalles para edición
            DetallesPedido = pedido.Pedido_Detalles.Select(pd => new PedidoDetalleDto
            {
                Codigo_Producto = pd.Codigo_Producto,
                Cantidad = pd.Cantidad,
                Precio_Unitario = pd.Precio_Unitario,
                Producto_Descripcion = pd.Producto.Descripcion
            }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadSelectListsAsync();
            
            // Remover validaciones de propiedades de navegación que no se envían desde el formulario
            ModelState.Remove("Pedido.Cliente");
            ModelState.Remove("Pedido.Agente_Ventas");
            
            // Remover validaciones de propiedades calculadas/navegación en detalles
            for (int i = 0; i < (DetallesPedido?.Count ?? 0); i++)
            {
                ModelState.Remove($"DetallesPedido[{i}].Producto_Descripcion");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Validar que tenga al menos un detalle
            if (DetallesPedido == null || !DetallesPedido.Any())
            {
                ModelState.AddModelError("", "El pedido debe tener al menos un producto");
                return Page();
            }

            // Validar inventario disponible para reserva
            var validacionInventario = await ValidarInventarioDisponible();
            if (!validacionInventario.IsValid)
            {
                ModelState.AddModelError("", validacionInventario.ErrorMessage);
                return Page();
            }

            // Calcular totales
            CalcularTotales();

            try
            {
                if (Pedido.Id == 0)
                {
                    // Crear nuevo pedido
                    await CrearNuevoPedido();
                }
                else
                {
                    // Actualizar pedido existente
                    await ActualizarPedidoExistente();
                }

                TempData["SuccessMessage"] = Pedido.Id == 0 ? "Pedido creado correctamente" : "Pedido actualizado correctamente";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar el pedido: {ex.Message}");
                return Page();
            }
        }

        private async Task CrearNuevoPedido()
        {
            _context.Pedido.Add(Pedido);
            await _context.SaveChangesAsync();

            // Agregar detalles
            foreach (var detalle in DetallesPedido)
            {
                var pedidoDetalle = new Pedido_Detalle
                {
                    Id_Pedido = Pedido.Id,
                    Codigo_Producto = detalle.Codigo_Producto,
                    Cantidad = detalle.Cantidad,
                    Precio_Unitario = detalle.Precio_Unitario
                };

                _context.Pedido_Detalle.Add(pedidoDetalle);

                // RESERVAR productos en inventario (no reducir existencia)
                await ReservarInventario(detalle.Codigo_Producto, detalle.Cantidad);
            }

            await _context.SaveChangesAsync();
        }

        private async Task ActualizarPedidoExistente()
        {
            // Obtener detalles actuales para liberar reservas
            var detallesActuales = await _context.Pedido_Detalle
                .Where(pd => pd.Id_Pedido == Pedido.Id)
                .ToListAsync();

            // Liberar reservas de los productos actuales
            foreach (var detalle in detallesActuales)
            {
                await LiberarReserva(detalle.Codigo_Producto, detalle.Cantidad);
            }

            // Eliminar detalles actuales
            _context.Pedido_Detalle.RemoveRange(detallesActuales);

            // Actualizar pedido
            _context.Update(Pedido);

            // Agregar nuevos detalles
            foreach (var detalle in DetallesPedido)
            {
                var pedidoDetalle = new Pedido_Detalle
                {
                    Id_Pedido = Pedido.Id,
                    Codigo_Producto = detalle.Codigo_Producto,
                    Cantidad = detalle.Cantidad,
                    Precio_Unitario = detalle.Precio_Unitario
                };

                _context.Pedido_Detalle.Add(pedidoDetalle);

                // RESERVAR productos en inventario
                await ReservarInventario(detalle.Codigo_Producto, detalle.Cantidad);
            }

            await _context.SaveChangesAsync();
        }

        private async Task ReservarInventario(string codigoProducto, int cantidad)
        {
            var inventario = await _context.Inventario
                .FirstOrDefaultAsync(i => i.Codigo_Producto == codigoProducto);

            if (inventario != null)
            {
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
                // Asegurar que las reservas no sean negativas
                if (inventario.Reservado < 0)
                    inventario.Reservado = 0;
                _context.Update(inventario);
            }
        }

        private async Task<(bool IsValid, string ErrorMessage)> ValidarInventarioDisponible()
        {
            foreach (var detalle in DetallesPedido)
            {
                var inventario = await _context.Inventario
                    .Include(i => i.Producto)
                    .FirstOrDefaultAsync(i => i.Codigo_Producto == detalle.Codigo_Producto);

                if (inventario == null)
                {
                    return (false, $"Producto {detalle.Codigo_Producto} no encontrado en inventario");
                }

                var disponible = inventario.Disponible; // Existencia - Reservado

                // Si estamos editando, sumar la cantidad actualmente reservada del pedido
                if (Pedido.Id > 0)
                {
                    var detalleActual = await _context.Pedido_Detalle
                        .FirstOrDefaultAsync(pd => pd.Id_Pedido == Pedido.Id && pd.Codigo_Producto == detalle.Codigo_Producto);

                    if (detalleActual != null)
                    {
                        disponible += detalleActual.Cantidad;
                    }
                }

                if (disponible < detalle.Cantidad)
                {
                    return (false, $"Stock insuficiente para {inventario.Producto.Descripcion}. Disponible: {disponible}, Requerido: {detalle.Cantidad}");
                }
            }

            return (true, string.Empty);
        }

        private void CalcularTotales()
        {
            Pedido.Total = DetallesPedido.Sum(d => d.Cantidad * d.Precio_Unitario);
            Pedido.Saldo = Pedido.Total - Pedido.Abono;
        }

        private async Task LoadSelectListsAsync()
        {
            // Cargar clientes con nombre completo (nombre + apellidos)
            var clientes = await _context.Cliente
                .Select(c => new { 
                    c.Id, 
                    NombreCompleto = $"{c.Nombre} {c.Primer_Apellido} {c.Segundo_Apellido}"
                })
                .ToListAsync();

            ClientesList = new SelectList(clientes, "Id", "NombreCompleto");

            // Mostrar solo productos con stock disponible (Existencia - Reservado > 0)
            ProductosDisponibles = await _context.Inventario
                .Include(i => i.Producto)
                .Where(i => i.Estado && (i.Existencia - i.Reservado) > 0)
                .Select(i => new ProductoInventarioDto
                {
                    Codigo = i.Codigo_Producto,
                    Descripcion = i.Producto.Descripcion,
                    Existencia = i.Existencia - i.Reservado, // Mostrar solo disponible
                    Precio = i.Precio_Venta
                })
                .ToListAsync();
        }

        public async Task<IActionResult> OnGetObtenerProductoAsync(string codigo)
        {
            var inventario = await _context.Inventario
                .Include(i => i.Producto)
                .FirstOrDefaultAsync(i => i.Codigo_Producto == codigo);

            if (inventario == null)
            {
                return NotFound();
            }

            var producto = new
            {
                codigo = inventario.Codigo_Producto,
                descripcion = inventario.Producto.Descripcion,
                existencia = inventario.Disponible,
                precio = inventario.Precio_Venta
            };

            return new JsonResult(producto);
        }

        public async Task<IActionResult> OnGetProductosInventarioAsync()
        {
            try
            {
                // Obtener productos con información de inventario
                var productosConInventario = await _context.Producto
                    .Include(p => p.Impuesto)
                    .Where(p => p.Estado == true)
                    .Select(p => new
                    {
                        codigo = p.CodigoReferencia,
                        nombre = p.Descripcion,
                        descripcion = p.Descripcion,
                        precio = _context.Inventario
                            .Where(i => i.Codigo_Producto == p.CodigoReferencia)
                            .Select(i => i.Precio_Venta)
                            .FirstOrDefault() > 0 ? _context.Inventario
                            .Where(i => i.Codigo_Producto == p.CodigoReferencia)
                            .Select(i => i.Precio_Venta)
                            .FirstOrDefault() : 0,
                        cantidadInventario = _context.Inventario
                            .Where(i => i.Codigo_Producto == p.CodigoReferencia)
                            .Sum(i => i.Existencia),
                        porcentajeImpuesto = p.Impuesto != null ? p.Impuesto.Porcentaje : 0
                    })
                    .OrderBy(p => p.nombre)
                    .ToListAsync();

                return new JsonResult(new { success = true, productos = productosConInventario });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error al cargar productos: " + ex.Message });
            }
        }
    }

    public class PedidoDetalleDto
    {
        [Required(ErrorMessage = "Seleccione un producto")]
        public string Codigo_Producto { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio_Unitario { get; set; }

        public string? Producto_Descripcion { get; set; }
    }

    public class ProductoInventarioDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Existencia { get; set; }
        public decimal Precio { get; set; }
    }
}