using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Data;
using Microsoft.EntityFrameworkCore;
using TOHPO.Models;

namespace TOHPO.Pages.Configuracion.Proveedores
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public IndexModel(AppDbContext context) { _context = context; }
        public void OnGet() { }

        // Handler para DataTables
        public async Task<JsonResult> OnGetProveedoresAsync()
        {
            try
            {
                var proveedores = await _context.Proveedor
                    .OrderBy(p => p.Nombre)
                    .Select(p => new { 
                        id = p.Id, 
                        nombre = p.Nombre, 
                        telefono = p.Telefono, 
                        correo = p.Correo_Electronico, 
                        direccion = p.Direccion,
                        estado = p.Estado
                    })
                    .ToListAsync();

                return new JsonResult(proveedores);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al cargar proveedores: {ex.Message}" });
            }
        }

        // Handler para crear un proveedor
        public async Task<IActionResult> OnPostCrearAsync([FromForm] string nombre, [FromForm] string telefono, [FromForm] string correo_Electronico, [FromForm] string direccion)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    return new JsonResult(new { success = false, message = "El nombre es requerido", type = "error" });
                }

                // Verificar si ya existe un proveedor con el mismo nombre
                var proveedorExistente = await _context.Proveedor
                    .AnyAsync(p => p.Nombre.ToLower() == nombre.ToLower());

                if (proveedorExistente)
                {
                    return new JsonResult(new { success = false, message = "Ya existe un proveedor con ese nombre", type = "error" });
                }

                // Verificar si ya existe un proveedor con el mismo correo electrónico
                if (!string.IsNullOrWhiteSpace(correo_Electronico))
                {
                    var proveedorConCorreo = await _context.Proveedor
                        .AnyAsync(p => p.Correo_Electronico.ToLower() == correo_Electronico.ToLower());

                    if (proveedorConCorreo)
                    {
                        return new JsonResult(new { success = false, message = "Ya existe un proveedor con ese correo electrónico", type = "error" });
                    }
                }

                var nuevoProveedor = new Proveedor
                {
                    Nombre = nombre.Trim(),
                    Telefono = telefono?.Trim(),
                    Correo_Electronico = correo_Electronico?.Trim(),
                    Direccion = direccion?.Trim(),
                    Estado = true
                };

                _context.Proveedor.Add(nuevoProveedor);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Proveedor creado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al crear proveedor: {ex.Message}", type = "error" });
            }
        }

        // Handler para editar un proveedor
        public async Task<IActionResult> OnPostEditarAsync([FromForm] int id, [FromForm] string nombre, [FromForm] string telefono, [FromForm] string correo_Electronico, [FromForm] string direccion, [FromForm] bool estado)
        {
            try
            {
                var proveedor = await _context.Proveedor.FindAsync(id);
                if (proveedor == null)
                {
                    return new JsonResult(new { success = false, message = "Proveedor no encontrado", type = "error" });
                }

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    return new JsonResult(new { success = false, message = "El nombre es requerido", type = "error" });
                }

                // Verificar si ya existe otro proveedor con el mismo nombre
                var proveedorExistente = await _context.Proveedor
                    .AnyAsync(p => p.Nombre.ToLower() == nombre.ToLower() && p.Id != id);

                if (proveedorExistente)
                {
                    return new JsonResult(new { success = false, message = "Ya existe otro proveedor con ese nombre", type = "error" });
                }

                // Verificar si ya existe otro proveedor con el mismo correo electrónico
                if (!string.IsNullOrWhiteSpace(correo_Electronico))
                {
                    var proveedorConCorreo = await _context.Proveedor
                        .AnyAsync(p => p.Correo_Electronico.ToLower() == correo_Electronico.ToLower() && p.Id != id);

                    if (proveedorConCorreo)
                    {
                        return new JsonResult(new { success = false, message = "Ya existe otro proveedor con ese correo electrónico", type = "error" });
                    }
                }

                proveedor.Nombre = nombre.Trim();
                proveedor.Telefono = telefono?.Trim();
                proveedor.Correo_Electronico = correo_Electronico?.Trim();
                proveedor.Direccion = direccion?.Trim();
                proveedor.Estado = estado;

                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Proveedor actualizado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al actualizar proveedor: {ex.Message}", type = "error" });
            }
        }

        // Handler para eliminar un proveedor
        public async Task<IActionResult> OnPostEliminarAsync([FromForm] int id)
        {
            try
            {
                var proveedor = await _context.Proveedor.FindAsync(id);
                if (proveedor == null)
                {
                    return new JsonResult(new { success = false, message = "Proveedor no encontrado", type = "error" });
                }

                // Verificar si el proveedor está siendo usado en compras
                var comprasConProveedor = await _context.Compra
                    .AnyAsync(c => c.Id_Proveedor == id);

                // Verificar si tiene agentes de venta asociados
                var agentesConProveedor = await _context.Agente_Ventas
                    .AnyAsync(a => a.Id_Proveedor == id);

                // Verificar si tiene productos asociados
                var productosConProveedor = await _context.Producto_Proveedor
                    .AnyAsync(pp => pp.Id_Proveedor == id);

                if (comprasConProveedor || agentesConProveedor || productosConProveedor)
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = "No se puede eliminar el proveedor porque tiene compras, agentes o productos asociados. ¿Desea desactivarlo en su lugar?", 
                        type = "validation",
                        canDelete = false,
                        shouldDeactivate = true
                    });
                }

                _context.Proveedor.Remove(proveedor);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Proveedor eliminado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al eliminar proveedor: {ex.Message}", type = "error" });
            }
        }

        // Handler para desactivar un proveedor
        public async Task<IActionResult> OnPostDesactivarAsync([FromForm] int id)
        {
            try
            {
                var proveedor = await _context.Proveedor.FindAsync(id);
                if (proveedor == null)
                {
                    return new JsonResult(new { success = false, message = "Proveedor no encontrado", type = "error" });
                }

                proveedor.Estado = false;
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Proveedor desactivado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al desactivar proveedor: {ex.Message}", type = "error" });
            }
        }
    }
}
