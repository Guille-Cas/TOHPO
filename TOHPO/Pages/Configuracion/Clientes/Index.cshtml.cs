using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TOHPO.Data;
using TOHPO.Models;

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
        public async Task<JsonResult> OnGetClientesAsync()
        {
            try
            {
                var clientes = await _context.Cliente
                    .OrderBy(c => c.Nombre)
                    .Select(c => new {
                        id = c.Id,
                        cedula = c.Cedula,
                        nombre = c.Nombre,
                        primer_Apellido = c.Primer_Apellido,
                        segundo_Apellido = c.Segundo_Apellido,
                        correo_Electronico = c.Correo_Electronico,
                        telefono = c.Telefono,
                        estado = c.Estado
                    }).ToListAsync();

                return new JsonResult(clientes);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al cargar clientes: {ex.Message}" });
            }
        }

        // Handler para crear cliente
        public async Task<IActionResult> OnPostCrearAsync([FromForm] Cliente cliente)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errores = string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    return new JsonResult(new { success = false, message = $"Datos inválidos: {errores}", type = "error" });
                }

                // Verificar si ya existe un cliente con la misma cédula
                var clienteExistente = await _context.Cliente
                    .AnyAsync(c => c.Cedula == cliente.Cedula);

                if (clienteExistente)
                {
                    return new JsonResult(new { success = false, message = "Ya existe un cliente con esa cédula", type = "error" });
                }

                cliente.Estado = true; // Por defecto activo
                _context.Cliente.Add(cliente);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Cliente creado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al crear cliente: {ex.Message}", type = "error" });
            }
        }

        // Handler para editar cliente
        public async Task<IActionResult> OnPostEditarAsync([FromForm] Cliente cliente)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errores = string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    return new JsonResult(new { success = false, message = $"Datos inválidos: {errores}", type = "error" });
                }

                var clienteExistente = await _context.Cliente.FindAsync(cliente.Id);
                if (clienteExistente == null)
                {
                    return new JsonResult(new { success = false, message = "Cliente no encontrado", type = "error" });
                }

                // Verificar si la cédula ya existe en otro cliente
                var cedulaExiste = await _context.Cliente
                    .AnyAsync(c => c.Cedula == cliente.Cedula && c.Id != cliente.Id);

                if (cedulaExiste)
                {
                    return new JsonResult(new { success = false, message = "Ya existe otro cliente con esa cédula", type = "error" });
                }

                // Actualizar propiedades
                clienteExistente.Nombre = cliente.Nombre;
                clienteExistente.Primer_Apellido = cliente.Primer_Apellido;
                clienteExistente.Segundo_Apellido = cliente.Segundo_Apellido;
                clienteExistente.Correo_Electronico = cliente.Correo_Electronico;
                clienteExistente.Telefono = cliente.Telefono;
                clienteExistente.Cedula = cliente.Cedula;
                clienteExistente.Estado = cliente.Estado;

                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Cliente actualizado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al actualizar cliente: {ex.Message}", type = "error" });
            }
        }

        // Handler para eliminar cliente
        public async Task<IActionResult> OnPostEliminarAsync([FromForm] int id)
        {
            try
            {
                var cliente = await _context.Cliente.FindAsync(id);
                if (cliente == null)
                {
                    return new JsonResult(new { success = false, message = "Cliente no encontrado", type = "error" });
                }

                // Verificar relaciones del cliente
                var relacionesEncontradas = new List<string>();

                // Verificar ventas
                var tieneVentas = await _context.Venta.AnyAsync(v => v.Id_Cliente == id);
                if (tieneVentas) relacionesEncontradas.Add("Ventas");

                // Verificar pedidos
                var tienePedidos = await _context.Pedido.AnyAsync(p => p.Id_Cliente == id);
                if (tienePedidos) relacionesEncontradas.Add("Pedidos");

                // Verificar recordatorios
                var tieneRecordatorios = await _context.Recordatorio.AnyAsync(r => r.ClienteId == id);
                if (tieneRecordatorios) relacionesEncontradas.Add("Recordatorios");

                if (relacionesEncontradas.Any())
                {
                    var mensaje = $"No se puede eliminar el cliente porque tiene registros relacionados en: {string.Join(", ", relacionesEncontradas)}. ¿Desea desactivarlo en su lugar?";
                    return new JsonResult(new { 
                        success = false, 
                        message = mensaje, 
                        type = "validation",
                        canDelete = false,
                        shouldDeactivate = true
                    });
                }

                _context.Cliente.Remove(cliente);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Cliente eliminado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al eliminar cliente: {ex.Message}", type = "error" });
            }
        }

        // Handler para desactivar cliente
        public async Task<IActionResult> OnPostDesactivarAsync([FromForm] int id)
        {
            try
            {
                var cliente = await _context.Cliente.FindAsync(id);
                if (cliente == null)
                {
                    return new JsonResult(new { success = false, message = "Cliente no encontrado", type = "error" });
                }

                cliente.Estado = false;
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Cliente desactivado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al desactivar cliente: {ex.Message}", type = "error" });
            }
        }

        // Handler para activar cliente
        public async Task<IActionResult> OnPostActivarAsync([FromForm] int id)
        {
            try
            {
                var cliente = await _context.Cliente.FindAsync(id);
                if (cliente == null)
                {
                    return new JsonResult(new { success = false, message = "Cliente no encontrado", type = "error" });
                }

                cliente.Estado = true;
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Cliente activado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al activar cliente: {ex.Message}", type = "error" });
            }
        }
    }
}
