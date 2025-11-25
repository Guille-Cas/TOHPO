using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Data;
using Microsoft.EntityFrameworkCore;

namespace TOHPO.Pages.Configuracion.Metodos_Pago
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public IndexModel(AppDbContext context) { _context = context; }
        public void OnGet() { }

        public JsonResult OnGetMetodosPago()
        {
            var metodos = _context.Metodo_Pago
                .Select(m => new { id = m.Id, descripcion = m.Descripcion })
                .ToList();
            return new JsonResult(metodos);
        }

        public IActionResult OnPostEliminar([FromForm] int id)
        {
            var metodo = _context.Metodo_Pago.Find(id);
            if (metodo == null) return new JsonResult(new { success = false });
            _context.Metodo_Pago.Remove(metodo);
            _context.SaveChanges();
            return new JsonResult(new { success = true });
        }
    }
}
