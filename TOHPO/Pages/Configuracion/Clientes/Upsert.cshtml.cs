using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Models;
using TOHPO.Data;

namespace TOHPO.Pages.Configuracion.Clientes
{
    public class UpsertModel : PageModel
    {
        private readonly AppDbContext _context;

        public UpsertModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Cliente Cliente { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id.HasValue)
            {
                Cliente = await _context.Cliente.FindAsync(id.Value);
                if (Cliente == null)
                {
                    return NotFound();
                }
            }
            else
            {
                Cliente = new Cliente();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Cliente.Id > 0)
            {
                var clienteExistente = await _context.Cliente.FindAsync(Cliente.Id);
                if (clienteExistente == null)
                {
                    return NotFound();
                }

                clienteExistente.Cedula = Cliente.Cedula;
                clienteExistente.Nombre = Cliente.Nombre;
                clienteExistente.Primer_Apellido = Cliente.Primer_Apellido;
                clienteExistente.Segundo_Apellido = Cliente.Segundo_Apellido;
                clienteExistente.Correo_Electronico = Cliente.Correo_Electronico;
                clienteExistente.Telefono = Cliente.Telefono;

                _context.Cliente.Update(clienteExistente);
            }
            else
            {
                _context.Cliente.Add(Cliente);
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("/Configuracion/Clientes/Index");
        }
    }
}
