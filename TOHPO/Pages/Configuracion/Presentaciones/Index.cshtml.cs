using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Data;
using Microsoft.EntityFrameworkCore;

namespace TOHPO.Pages.Configuracion.Presentaciones
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public IndexModel(AppDbContext context) { _context = context; }
        public void OnGet() { }

        public JsonResult OnGetPresentaciones()
        {
            var presentaciones = _context.Presentacion
                .Select(p => new { id = p.Id, cantidad = p.Cantidad, unidad_Medida = p.Unidad_Medida.ToString() })
                .ToList();
            return new JsonResult(presentaciones);
        }

        public IActionResult OnPostEliminar([FromForm] int id)
        {
            var presentacion = _context.Presentacion.Find(id);
            if (presentacion == null) return new JsonResult(new { success = false });
            _context.Presentacion.Remove(presentacion);
            _context.SaveChanges();
            return new JsonResult(new { success = true });
        }
    }
}
