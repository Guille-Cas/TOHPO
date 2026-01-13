using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Models;
using TOHPO.Data;
using Microsoft.EntityFrameworkCore;

namespace TOHPO.Pages.Configuracion.Metodos_Pago
{
    public class UpsertModel : PageModel
    {
        private readonly AppDbContext _context;
        public UpsertModel(AppDbContext context) { _context = context; }

        [BindProperty]
        public Metodo_Pago MetodoPago { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            try
            {
                if (id.HasValue)
                {
                    MetodoPago = await _context.Metodo_Pago.FindAsync(id.Value);
                    if (MetodoPago == null) 
                    {
                        TempData["error"] = "Método de pago no encontrado";
                        return RedirectToPage("/Configuracion/Metodos_Pago/Index");
                    }
                }
                else
                {
                    MetodoPago = new Metodo_Pago { Estado = true }; // Por defecto activo para nuevos métodos de pago
                }
                return Page();
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error al cargar el método de pago: {ex.Message}";
                return RedirectToPage("/Configuracion/Metodos_Pago/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["error"] = "Por favor corrija los errores en el formulario";
                    return Page();
                }

                // Verificar si ya existe un método de pago con la misma descripción
                var metodoExistente = await _context.Metodo_Pago
                    .AnyAsync(m => m.Descripcion.ToLower() == MetodoPago.Descripcion.ToLower() && m.Id != MetodoPago.Id);

                if (metodoExistente)
                {
                    TempData["error"] = "Ya existe un método de pago con esa descripción";
                    return Page();
                }
                
                if (MetodoPago.Id > 0)
                {
                    var existente = await _context.Metodo_Pago.FindAsync(MetodoPago.Id);
                    if (existente == null) 
                    {
                        TempData["error"] = "Método de pago no encontrado";
                        return RedirectToPage("/Configuracion/Metodos_Pago/Index");
                    }
                    
                    // Actualizar todas las propiedades
                    existente.Descripcion = MetodoPago.Descripcion?.Trim();
                    existente.Estado = MetodoPago.Estado;
                    
                    _context.Metodo_Pago.Update(existente);
                    TempData["success"] = "Método de pago actualizado exitosamente";
                }
                else
                {
                    MetodoPago.Descripcion = MetodoPago.Descripcion?.Trim();
                    _context.Metodo_Pago.Add(MetodoPago);
                    TempData["success"] = "Método de pago creado exitosamente";
                }
                
                await _context.SaveChangesAsync();
                return RedirectToPage("/Configuracion/Metodos_Pago/Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error al guardar el método de pago: {ex.Message}";
                return Page();
            }
        }
    }
}
