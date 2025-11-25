using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Data;
using Microsoft.EntityFrameworkCore;

namespace TOHPO.Pages.Configuracion.Impuestos
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public IndexModel(AppDbContext context) { _context = context; }
        public void OnGet() { }

        public JsonResult OnGetImpuestos()
        {
            var impuestos = _context.Impuesto
                .Select(i => new { id = i.Id, descripcion = i.Descripcion, porcentaje = i.Porcentaje })
                .ToList();
            return new JsonResult(impuestos);
        }

        public IActionResult OnPostEliminar([FromForm] int id)
        {
            var impuesto = _context.Impuesto.Find(id);
            if (impuesto == null) return new JsonResult(new { success = false });
            _context.Impuesto.Remove(impuesto);
            _context.SaveChanges();
            return new JsonResult(new { success = true });
        }
    }
}
