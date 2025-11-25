using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Models;
using TOHPO.Data;

namespace TOHPO.Pages.Configuracion.Presentaciones
{
    public class UpsertModel : PageModel
    {
        private readonly AppDbContext _context;
        public UpsertModel(AppDbContext context) { _context = context; }

        [BindProperty]
        public Presentacion Presentacion { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id.HasValue)
            {
                Presentacion = await _context.Presentacion.FindAsync(id.Value);
                if (Presentacion == null) return NotFound();
            }
            else
            {
                Presentacion = new Presentacion();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            if (Presentacion.Id > 0)
            {
                var existente = await _context.Presentacion.FindAsync(Presentacion.Id);
                if (existente == null) return NotFound();
                existente.Cantidad = Presentacion.Cantidad;
                existente.Unidad_Medida = Presentacion.Unidad_Medida;
                _context.Presentacion.Update(existente);
            }
            else
            {
                _context.Presentacion.Add(Presentacion);
            }
            await _context.SaveChangesAsync();
            return RedirectToPage("/Configuracion/Presentaciones/Index");
        }
    }
}
