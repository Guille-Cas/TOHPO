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
            try
            {
                Proveedores = await _context.Proveedor.Where(p => p.Estado).ToListAsync();
                
                if (id.HasValue)
                {
                    AgenteVentas = await _context.Agente_Ventas
                        .Include(a => a.Proveedor)
                        .FirstOrDefaultAsync(a => a.Id == id.Value);
                    
                    if (AgenteVentas == null) 
                    {
                        TempData["error"] = "Agente de ventas no encontrado";
                        return RedirectToPage("/Configuracion/Agentes_ventas/Index");
                    }
                    ProveedorDescripcion = AgenteVentas.Proveedor?.Nombre ?? "";
                }
                else
                {
                    AgenteVentas = new Agente_Ventas { Estado = true }; // Por defecto activo para nuevos agentes
                }
                return Page();
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error al cargar el agente de ventas: {ex.Message}";
                return RedirectToPage("/Configuracion/Agentes_ventas/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                Proveedores = await _context.Proveedor.Where(p => p.Estado).ToListAsync();
                ModelState.Remove("AgenteVentas.Proveedor");
                
                if (!ModelState.IsValid)
                {
                    TempData["error"] = "Por favor corrija los errores en el formulario";
                    return Page();
                }

                // Validar correo electrónico único si se proporciona
                if (!string.IsNullOrWhiteSpace(AgenteVentas.Correo_Electronico))
                {
                    var agenteExistente = await _context.Agente_Ventas
                        .AnyAsync(a => a.Correo_Electronico.ToLower() == AgenteVentas.Correo_Electronico.ToLower() && a.Id != AgenteVentas.Id);

                    if (agenteExistente)
                    {
                        TempData["error"] = "Ya existe un agente con ese correo electrónico";
                        return Page();
                    }
                }

                // Validar que el proveedor existe si se seleccionó
                if (AgenteVentas.Id_Proveedor > 0)
                {
                    var proveedorExiste = await _context.Proveedor.AnyAsync(p => p.Id == AgenteVentas.Id_Proveedor);
                    if (!proveedorExiste)
                    {
                        TempData["error"] = "El proveedor seleccionado no existe";
                        return Page();
                    }
                }
                
                if (AgenteVentas.Id > 0)
                {
                    var existente = await _context.Agente_Ventas.FindAsync(AgenteVentas.Id);
                    if (existente == null) 
                    {
                        TempData["error"] = "Agente de ventas no encontrado";
                        return RedirectToPage("/Configuracion/Agentes_ventas/Index");
                    }
                    
                    // Actualizar todas las propiedades
                    existente.Nombre = AgenteVentas.Nombre?.Trim();
                    existente.Telefono = AgenteVentas.Telefono?.Trim();
                    existente.Correo_Electronico = AgenteVentas.Correo_Electronico?.Trim();
                    existente.Id_Proveedor = AgenteVentas.Id_Proveedor;
                    existente.Estado = AgenteVentas.Estado; 

                    _context.Agente_Ventas.Update(existente);
                    TempData["success"] = "Agente de ventas actualizado exitosamente";
                }
                else
                {
                    AgenteVentas.Nombre = AgenteVentas.Nombre?.Trim();
                    AgenteVentas.Telefono = AgenteVentas.Telefono?.Trim();
                    AgenteVentas.Correo_Electronico = AgenteVentas.Correo_Electronico?.Trim();
                    
                    _context.Agente_Ventas.Add(AgenteVentas);
                    TempData["success"] = "Agente de ventas creado exitosamente";
                }
                
                await _context.SaveChangesAsync();
                return RedirectToPage("/Configuracion/Agentes_ventas/Index");
            }
            catch (Exception ex)
            {
                Proveedores = await _context.Proveedor.Where(p => p.Estado).ToListAsync();
                TempData["success"] = $"Error al guardar el agente de ventas: {ex.Message}";
                return Page();
            }
        }
    }
}
