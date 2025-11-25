using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Data;
using Microsoft.EntityFrameworkCore;

namespace TOHPO.Pages.Configuracion.Proveedores
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public IndexModel(AppDbContext context) { _context = context; }
        public void OnGet() { }

        public JsonResult OnGetProveedores()
        {
            var proveedores = _context.Proveedor
                .Select(p => new { id = p.Id, nombre = p.Nombre, telefono = p.Telefono, correo = p.Correo_Electronico, direccion = p.Direccion })
                .ToList();
            return new JsonResult(proveedores);
        }

        public IActionResult OnPostEliminar([FromForm] int id)
        {
            var proveedor = _context.Proveedor.Find(id);
            if (proveedor == null) return new JsonResult(new { success = false });
            _context.Proveedor.Remove(proveedor);
            _context.SaveChanges();
            return new JsonResult(new { success = true });
        }
    }
}
