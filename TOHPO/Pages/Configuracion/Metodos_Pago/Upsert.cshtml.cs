using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Models;
using TOHPO.Data;

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
            if (id.HasValue)
            {
                MetodoPago = await _context.Metodo_Pago.FindAsync(id.Value);
                if (MetodoPago == null) return NotFound();
            }
            else
            {
                MetodoPago = new Metodo_Pago();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            if (MetodoPago.Id > 0)
            {
                var existente = await _context.Metodo_Pago.FindAsync(MetodoPago.Id);
                if (existente == null) return NotFound();
                existente.Descripcion = MetodoPago.Descripcion;
                _context.Metodo_Pago.Update(existente);
            }
            else
            {
                _context.Metodo_Pago.Add(MetodoPago);
            }
            await _context.SaveChangesAsync();
            return RedirectToPage("/Configuracion/Metodos_Pago/Index");
        }
    }
}
