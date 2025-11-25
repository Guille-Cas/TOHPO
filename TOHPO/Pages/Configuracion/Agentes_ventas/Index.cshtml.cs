using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Data;
using Microsoft.EntityFrameworkCore;

namespace TOHPO.Pages.Configuracion.Agentes_ventas
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public IndexModel(AppDbContext context) { _context = context; }
        public void OnGet() { }

        public JsonResult OnGetAgentesVentas()
        {
            var agentes = _context.Agente_Ventas.Include(a => a.Proveedor)
                .Select(a => new {
                    id = a.Id,
                    nombre = a.Nombre,
                    telefono = a.Telefono,
                    correo_Electronico = a.Correo_Electronico,
                    proveedor = a.Proveedor != null ? a.Proveedor.Nombre : ""
                }).ToList();
            return new JsonResult(agentes);
        }

        public IActionResult OnPostEliminar([FromForm] int id)
        {
            var agente = _context.Agente_Ventas.Find(id);
            if (agente == null) return new JsonResult(new { success = false });
            _context.Agente_Ventas.Remove(agente);
            _context.SaveChanges();
            return new JsonResult(new { success = true });
        }
    }
}
