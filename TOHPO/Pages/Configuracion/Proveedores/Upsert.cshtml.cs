using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Models;
using TOHPO.Data;
using Microsoft.EntityFrameworkCore;

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
            try
            {
                if (id.HasValue)
                {
                    Proveedor = await _context.Proveedor.FindAsync(id.Value);
                    if (Proveedor == null) 
                    {
                        TempData["ErrorMessage"] = "Proveedor no encontrado";
                        return RedirectToPage("/Configuracion/Proveedores/Index");
                    }
                }
                else
                {
                    Proveedor = new Proveedor { Estado = true }; // Por defecto activo para nuevos proveedores
                }
                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar el proveedor: {ex.Message}";
                return RedirectToPage("/Configuracion/Proveedores/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Por favor corrija los errores en el formulario";
                    return Page();
                }

                // Verificar si ya existe un proveedor con el mismo nombre
                var proveedorExistente = await _context.Proveedor
                    .AnyAsync(p => p.Nombre.ToLower() == Proveedor.Nombre.ToLower() && p.Id != Proveedor.Id);

                if (proveedorExistente)
                {
                    TempData["ErrorMessage"] = "Ya existe un proveedor con ese nombre";
                    return Page();
                }

                // Verificar si ya existe un proveedor con el mismo correo electrónico
                if (!string.IsNullOrWhiteSpace(Proveedor.Correo_Electronico))
                {
                    var proveedorConCorreo = await _context.Proveedor
                        .AnyAsync(p => p.Correo_Electronico.ToLower() == Proveedor.Correo_Electronico.ToLower() && p.Id != Proveedor.Id);

                    if (proveedorConCorreo)
                    {
                        TempData["ErrorMessage"] = "Ya existe un proveedor con ese correo electrónico";
                        return Page();
                    }
                }
                
                if (Proveedor.Id > 0)
                {
                    var existente = await _context.Proveedor.FindAsync(Proveedor.Id);
                    if (existente == null) 
                    {
                        TempData["ErrorMessage"] = "Proveedor no encontrado";
                        return RedirectToPage("/Configuracion/Proveedores/Index");
                    }
                    
                    // Actualizar todas las propiedades
                    existente.Nombre = Proveedor.Nombre?.Trim();
                    existente.Telefono = Proveedor.Telefono?.Trim();
                    existente.Correo_Electronico = Proveedor.Correo_Electronico?.Trim();
                    existente.Direccion = Proveedor.Direccion?.Trim();
                    existente.Estado = Proveedor.Estado;
                    
                    _context.Proveedor.Update(existente);
                    TempData["SuccessMessage"] = "Proveedor actualizado exitosamente";
                }
                else
                {
                    Proveedor.Nombre = Proveedor.Nombre?.Trim();
                    Proveedor.Telefono = Proveedor.Telefono?.Trim();
                    Proveedor.Correo_Electronico = Proveedor.Correo_Electronico?.Trim();
                    Proveedor.Direccion = Proveedor.Direccion?.Trim();
                    
                    _context.Proveedor.Add(Proveedor);
                    TempData["SuccessMessage"] = "Proveedor creado exitosamente";
                }
                
                await _context.SaveChangesAsync();
                return RedirectToPage("/Configuracion/Proveedores/Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al guardar el proveedor: {ex.Message}";
                return Page();
            }
        }
    }
}
