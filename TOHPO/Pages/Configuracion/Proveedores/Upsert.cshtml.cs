using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Models;
using TOHPO.Data;

namespace TOHPO.Pages.Configuracion.Proveedores
{
    public class UpsertModel : PageModel
    {
        private readonly AppDbContext _context;
        public UpsertModel(AppDbContext context) { _context = context; }

        [BindProperty]
        public Proveedor Proveedor { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id.HasValue)
            {
                Proveedor = await _context.Proveedor.FindAsync(id.Value);
                if (Proveedor == null) return NotFound();
            }
            else
            {
                Proveedor = new Proveedor { Estado = true }; // Por defecto activo para nuevos proveedores
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            
            if (Proveedor.Id > 0)
            {
                var existente = await _context.Proveedor.FindAsync(Proveedor.Id);
                if (existente == null) return NotFound();
                
                // Actualizar todas las propiedades
                existente.Nombre = Proveedor.Nombre;
                existente.Telefono = Proveedor.Telefono;
                existente.Correo_Electronico = Proveedor.Correo_Electronico;
                existente.Direccion = Proveedor.Direccion;
                existente.Estado = Proveedor.Estado; // ← AGREGAR ESTA LÍNEA
                
                _context.Proveedor.Update(existente);
            }
            else
            {
                _context.Proveedor.Add(Proveedor);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToPage("/Configuracion/Proveedores/Index");
        }
    }
}
