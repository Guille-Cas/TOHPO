using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Data;
using Microsoft.EntityFrameworkCore;

namespace TOHPO.Pages.Configuracion.Categorias
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public IndexModel(AppDbContext context) { _context = context; }
        public void OnGet() { }

        public JsonResult OnGetCategorias()
        {
            var categorias = _context.Categoria
                .Select(c => new { id = c.Id, descripcion = c.Descripcion })
                .ToList();
            return new JsonResult(categorias);
        }

        public IActionResult OnPostEliminar([FromForm] int id)
        {
            var categoria = _context.Categoria.Find(id);
            if (categoria == null) return new JsonResult(new { success = false });
            _context.Categoria.Remove(categoria);
            _context.SaveChanges();
            return new JsonResult(new { success = true });
        }
    }
}
