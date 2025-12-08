using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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
        public Cliente Cliente { get; set; } = new Cliente();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            try
            {
                if (id.HasValue)
                {
                    Cliente = await _context.Cliente.FindAsync(id.Value);
                    if (Cliente == null)
                    {
                        TempData["ErrorMessage"] = "Cliente no encontrado";
                        return RedirectToPage("./Index");
                    }
                }
                else
                {
                    Cliente = new Cliente { };
                }
                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar el cliente: {ex.Message}";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Validar cédula única
                var existeCliente = await _context.Cliente
                    .AnyAsync(c => c.Cedula == Cliente.Cedula && c.Id != Cliente.Id);

                if (existeCliente)
                {
                    ModelState.AddModelError("Cliente.Cedula", "Ya existe un cliente con esta cédula");
                }

                if (!ModelState.IsValid)
                {
                    return Page();
                }

                if (Cliente.Id > 0)
                {
                    var clienteExistente = await _context.Cliente.FindAsync(Cliente.Id);
                    if (clienteExistente == null)
                    {
                        TempData["ErrorMessage"] = "Cliente no encontrado";
                        return RedirectToPage("./Index");
                    }

                    // Actualizar propiedades
                    clienteExistente.Cedula = Cliente.Cedula;
                    clienteExistente.Nombre = Cliente.Nombre;
                    clienteExistente.Primer_Apellido = Cliente.Primer_Apellido;
                    clienteExistente.Segundo_Apellido = Cliente.Segundo_Apellido;
                    clienteExistente.Correo_Electronico = Cliente.Correo_Electronico;
                    clienteExistente.Telefono = Cliente.Telefono;
                    clienteExistente.Estado = Cliente.Estado;

                    _context.Cliente.Update(clienteExistente);
                    TempData["SuccessMessage"] = "Cliente actualizado correctamente";
                }
                else
                {
                    _context.Cliente.Add(Cliente);
                    TempData["SuccessMessage"] = "Cliente creado correctamente";
                }

                await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }
            catch (DbUpdateException dbEx)
            {
                ModelState.AddModelError("", $"Error de base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error inesperado: {ex.Message}");
                return Page();
            }
        }
    }
}
