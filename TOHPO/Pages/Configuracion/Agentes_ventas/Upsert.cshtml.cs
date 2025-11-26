using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Models;
using TOHPO.Data;
using Microsoft.EntityFrameworkCore;

namespace TOHPO.Pages.Configuracion.Agentes_ventas
{
    public class UpsertModel : PageModel
    {
        private readonly AppDbContext _context;
        public UpsertModel(AppDbContext context) { _context = context; }

        [BindProperty]
        public Agente_Ventas AgenteVentas { get; set; }
        public List<Proveedor> Proveedores { get; set; } = new();
        public string ProveedorDescripcion { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            Proveedores = await _context.Proveedor.ToListAsync();
            if (id.HasValue)
            {
                AgenteVentas = await _context.Agente_Ventas.Include(a => a.Proveedor).FirstOrDefaultAsync(a => a.Id == id.Value);
                if (AgenteVentas == null) return NotFound();
                ProveedorDescripcion = AgenteVentas.Proveedor?.Nombre ?? "";
            }
            else
            {
                AgenteVentas = new Agente_Ventas();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Proveedores = await _context.Proveedor.ToListAsync();
            ModelState.Remove("AgenteVentas.Proveedor");
            if (!ModelState.IsValid) return Page();
            if (AgenteVentas.Id > 0)
            {
                var existente = await _context.Agente_Ventas.FindAsync(AgenteVentas.Id);
                if (existente == null) return NotFound();
                existente.Nombre = AgenteVentas.Nombre;
                existente.Telefono = AgenteVentas.Telefono;
                existente.Correo_Electronico = AgenteVentas.Correo_Electronico;
                existente.Id_Proveedor = AgenteVentas.Id_Proveedor;
                _context.Agente_Ventas.Update(existente);
            }
            else
            {
                _context.Agente_Ventas.Add(AgenteVentas);
            }
            await _context.SaveChangesAsync();
            return RedirectToPage("/Configuracion/Agentes_ventas/Index");
        }
    }
}
