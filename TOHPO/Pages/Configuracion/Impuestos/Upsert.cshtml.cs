using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Data;
using TOHPO.Models;

namespace TOHPO.Pages.Configuracion.Impuestos
{
    public class UpsertModel : PageModel
    {
        private readonly AppDbContext _context;
        public UpsertModel(AppDbContext context) { _context = context; }

        [BindProperty]
        public Impuesto Impuesto { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id.HasValue)
            {
                Impuesto = await _context.Impuesto.FindAsync(id.Value);
                if (Impuesto == null) return NotFound();
            }
            else
            {
                Impuesto = new Impuesto();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            if (Impuesto.Id > 0)
            {
                var existente = await _context.Impuesto.FindAsync(Impuesto.Id);
                if (existente == null) return NotFound();
                existente.Descripcion = Impuesto.Descripcion;
                existente.Porcentaje = Impuesto.Porcentaje;
                _context.Impuesto.Update(existente);
            }
            else
            {
                _context.Impuesto.Add(Impuesto);
            }
            await _context.SaveChangesAsync();
            return RedirectToPage("/Configuracion/Impuestos/Index");
        }
    }
}
