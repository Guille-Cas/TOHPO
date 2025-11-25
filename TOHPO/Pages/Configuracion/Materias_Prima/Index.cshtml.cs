using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Data;
using Microsoft.EntityFrameworkCore;

namespace TOHPO.Pages.Configuracion.Materias_Prima
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public IndexModel(AppDbContext context) { _context = context; }
        public void OnGet() { }

        public JsonResult OnGetMateriasPrima()
        {
            var materias = _context.Materia_Prima
                .Select(m => new { id = m.Id, descripcion = m.Descripcion, unidad_Medida = m.Unidad_Medida.ToString() })
                .ToList();
            return new JsonResult(materias);
        }

        public IActionResult OnPostEliminar([FromForm] int id)
        {
            var materia = _context.Materia_Prima.Find(id);
            if (materia == null) return new JsonResult(new { success = false });
            _context.Materia_Prima.Remove(materia);
            _context.SaveChanges();
            return new JsonResult(new { success = true });
        }
    }
}
