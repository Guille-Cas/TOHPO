using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TOHPO.Data;

namespace TOHPO.Pages.Configuracion.Clientes
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
        }

        // Handler para DataTables
        public JsonResult OnGetClientes()
        {
            try
            {
                var clientes = _context.Cliente
                    .Select(c => new {
         id = c.Id,
         cedula = c.Cedula,
         nombre = c.Nombre,
         primer_Apellido = c.Primer_Apellido,
         segundo_Apellido = c.Segundo_Apellido,
         correo_Electronico = c.Correo_Electronico,
         telefono = c.Telefono
                }).ToList();

                return new JsonResult(clientes);
            }
            catch (Exception ex)
            {
                // Retorna el error para depuración
                return new JsonResult(new { error = ex.Message });
            }
        }

        // Handler para eliminar un cliente
        public IActionResult OnPostEliminar([FromForm] int id)
        {
            try
            {
                var cliente = _context.Cliente.Find(id);
                if (cliente == null)
                    return new JsonResult(new { success = false });

                _context.Cliente.Remove(cliente);
                _context.SaveChanges();

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }

    }
}
