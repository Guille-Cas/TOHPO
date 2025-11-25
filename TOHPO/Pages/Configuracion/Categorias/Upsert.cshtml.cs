using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Models;
using TOHPO.Data;

namespace TOHPO.Pages.Configuracion.Categorias
{
    public class UpsertModel : PageModel
    {
        private readonly AppDbContext _context;
        public UpsertModel(AppDbContext context) { _context = context; }

        [BindProperty]
        public Categoria Categoria { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id.HasValue)
            {
                Categoria = await _context.Categoria.FindAsync(id.Value);
                if (Categoria == null) return NotFound();
            }
            else
            {
                Categoria = new Categoria();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            if (Categoria.Id > 0)
            {
                var existente = await _context.Categoria.FindAsync(Categoria.Id);
                if (existente == null) return NotFound();
                existente.Descripcion = Categoria.Descripcion;
                _context.Categoria.Update(existente);
            }
            else
            {
                _context.Categoria.Add(Categoria);
            }
            await _context.SaveChangesAsync();
            return RedirectToPage("/Configuracion/Categorias/Index");
        }
    }
}
