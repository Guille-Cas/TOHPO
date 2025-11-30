using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TOHPO.Data;
using TOHPO.Models;

namespace TOHPO.Pages.Operaciones.Ventas
{
    public class UpsertModel : PageModel
    {
        private readonly AppDbContext _context;

        public UpsertModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Venta Venta { get; set; } = new Venta();

        [BindProperty]
        public List<DetalleVentaViewModel> DetallesVenta { get; set; } = new List<DetalleVentaViewModel>();

        public SelectList ClientesList { get; set; } = default!;
        public List<Producto> ProductosDisponibles { get; set; } = new List<Producto>();

        public class DetalleVentaViewModel
        {
            public int Id { get; set; }
            public string CodigoProducto { get; set; } = string.Empty;
            public string NombreProducto { get; set; } = string.Empty;
            public int Cantidad { get; set; } = 1;
            public decimal PrecioUnitario { get; set; }
            public decimal PorcentajeDescuento { get; set; }
            public decimal MontoDescuento { get; set; }
            public decimal MontoImpuesto { get; set; }
            public decimal Subtotal { get; set; }
            public decimal PorcentajeImpuesto { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            await CargarDatos();

            if (id.HasValue)
            {
                var venta = await _context.Venta
                    .Include(v => v.Detalle_Ventas)
                        .ThenInclude(dv => dv.Producto)
                            .ThenInclude(p => p.Impuesto)
                    .FirstOrDefaultAsync(v => v.Id == id.Value);

                if (venta == null)
                {
                    TempData["ErrorMessage"] = "Venta no encontrada";
                    return RedirectToPage("./Index");
                }

                Venta = venta;

                DetallesVenta = venta.Detalle_Ventas.Select(dv => new DetalleVentaViewModel
                {
                    Id = dv.Id,
                    CodigoProducto = dv.Codigo_Producto,
                    NombreProducto = dv.Producto.Descripcion,
                    Cantidad = dv.Cantidad,
                    PrecioUnitario = dv.Precio_Unitario,
                    PorcentajeDescuento = dv.Porcentaje_Descuento,
                    MontoDescuento = dv.Monto_Descuento,
                    MontoImpuesto = dv.Monto_Impuesto,
                    Subtotal = dv.Subtotal,
                    PorcentajeImpuesto = dv.Producto.Impuesto?.Porcentaje ?? 0
                }).ToList();
            }
            else
            {
                // Nueva venta
                Venta.Fecha = DateTime.Now.Date;
                Venta.Hora = DateTime.Now;

            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Venta.Cliente");
            ModelState.Remove("Venta.Agente_Ventas"); // Mantener para evitar errores
            ModelState.Remove("Venta.Concepto");
            
            
            if (!ModelState.IsValid)
            {
                await CargarDatos();
                return Page();
            }

            try
            {
                // Resto de la lógica de guardado sin agente de ventas
                // ... (continúa con el código existente pero sin referencias al agente)
                
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Venta guardada correctamente";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar la venta: {ex.Message}");
                await CargarDatos();
                return Page();
            }
        }

        private async Task CargarDatos()
        {
            // Cargar solo clientes, sin agentes
            var clientes = await _context.Cliente
                .OrderBy(c => c.Nombre)
                .Select(c => new { 
                    c.Id, 
                    NombreCompleto = $"{c.Nombre} {c.Primer_Apellido} {c.Segundo_Apellido}" 
                })
                .ToListAsync();
            
            ClientesList = new SelectList(clientes, "Id", "NombreCompleto");

            ProductosDisponibles = await _context.Producto
                .Where(p => p.Estado == true)
                .OrderBy(p => p.Descripcion)
                .ToListAsync();
        }
    }
}