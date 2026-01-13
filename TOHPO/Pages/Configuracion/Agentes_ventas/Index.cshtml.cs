using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Data;
using Microsoft.EntityFrameworkCore;
using TOHPO.Models;

namespace TOHPO.Pages.Configuracion.Agentes_ventas
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public IndexModel(AppDbContext context) { _context = context; }
        public void OnGet() { }

        // Handler para DataTables
        public async Task<JsonResult> OnGetAgentesVentasAsync()
        {
            try
            {
                var agentes = await _context.Agente_Ventas
                    .Include(a => a.Proveedor)
                    .OrderBy(a => a.Nombre)
                    .Select(a => new {
                        id = a.Id,
                        nombre = a.Nombre,
                        telefono = a.Telefono,
                        correo_Electronico = a.Correo_Electronico,
                        proveedor = a.Proveedor != null ? a.Proveedor.Nombre : "",
                        proveedorId = a.Id_Proveedor,
                        estado = a.Estado
                    }).ToListAsync();

                return new JsonResult(agentes);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al cargar agentes de ventas: {ex.Message}" });
            }
        }

        // Handler para crear un agente de ventas
        public async Task<IActionResult> OnPostCrearAsync([FromForm] string nombre, [FromForm] string telefono, [FromForm] string correo_Electronico, [FromForm] int? id_Proveedor)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    return new JsonResult(new { success = false, message = "El nombre es requerido", type = "error" });
                }

                // Verificar si ya existe un agente con el mismo correo electrónico
                if (!string.IsNullOrWhiteSpace(correo_Electronico))
                {
                    var agenteExistente = await _context.Agente_Ventas
                        .AnyAsync(a => a.Correo_Electronico.ToLower() == correo_Electronico.ToLower());

                    if (agenteExistente)
                    {
                        return new JsonResult(new { success = false, message = "Ya existe un agente con ese correo electrónico", type = "error" });
                    }
                }

                // Validar que el proveedor existe si se proporcionó
                if (id_Proveedor.HasValue && id_Proveedor > 0)
                {
                    var proveedorExiste = await _context.Proveedor.AnyAsync(p => p.Id == id_Proveedor);
                    if (!proveedorExiste)
                    {
                        return new JsonResult(new { success = false, message = "El proveedor seleccionado no existe", type = "error" });
                    }
                }

                var nuevoAgente = new Agente_Ventas
                {
                    Nombre = nombre.Trim(),
                    Telefono = telefono?.Trim(),
                    Correo_Electronico = correo_Electronico?.Trim(),
                    Id_Proveedor = id_Proveedor ?? 0,
                    Estado = true
                };

                _context.Agente_Ventas.Add(nuevoAgente);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Agente de ventas creado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al crear agente de ventas: {ex.Message}", type = "error" });
            }
        }

        // Handler para editar un agente de ventas
        public async Task<IActionResult> OnPostEditarAsync([FromForm] int id, [FromForm] string nombre, [FromForm] string telefono, [FromForm] string correo_Electronico, [FromForm] int? id_Proveedor, [FromForm] bool estado)
        {
            try
            {
                var agente = await _context.Agente_Ventas.FindAsync(id);
                if (agente == null)
                {
                    return new JsonResult(new { success = false, message = "Agente de ventas no encontrado", type = "error" });
                }

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    return new JsonResult(new { success = false, message = "El nombre es requerido", type = "error" });
                }

                // Verificar si ya existe otro agente con el mismo correo electrónico
                if (!string.IsNullOrWhiteSpace(correo_Electronico))
                {
                    var agenteExistente = await _context.Agente_Ventas
                        .AnyAsync(a => a.Correo_Electronico.ToLower() == correo_Electronico.ToLower() && a.Id != id);

                    if (agenteExistente)
                    {
                        return new JsonResult(new { success = false, message = "Ya existe otro agente con ese correo electrónico", type = "error" });
                    }
                }

                // Validar que el proveedor existe si se proporcionó
                if (id_Proveedor.HasValue && id_Proveedor > 0)
                {
                    var proveedorExiste = await _context.Proveedor.AnyAsync(p => p.Id == id_Proveedor);
                    if (!proveedorExiste)
                    {
                        return new JsonResult(new { success = false, message = "El proveedor seleccionado no existe", type = "error" });
                    }
                }

                agente.Nombre = nombre.Trim();
                agente.Telefono = telefono?.Trim();
                agente.Correo_Electronico = correo_Electronico?.Trim();
                agente.Id_Proveedor = id_Proveedor ?? 0;
                agente.Estado = estado;

                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Agente de ventas actualizado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al actualizar agente de ventas: {ex.Message}", type = "error" });
            }
        }

        // Handler para eliminar un agente de ventas
        public async Task<IActionResult> OnPostEliminarAsync([FromForm] int id)
        {
            try
            {
                var agente = await _context.Agente_Ventas.FindAsync(id);
                if (agente == null)
                {
                    return new JsonResult(new { success = false, message = "Agente de ventas no encontrado", type = "error" });
                }

                _context.Agente_Ventas.Remove(agente);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Agente de ventas eliminado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al eliminar agente de ventas: {ex.Message}", type = "error" });
            }
        }

        // Handler para desactivar un agente de ventas
        public async Task<IActionResult> OnPostDesactivarAsync([FromForm] int id)
        {
            try
            {
                var agente = await _context.Agente_Ventas.FindAsync(id);
                if (agente == null)
                {
                    return new JsonResult(new { success = false, message = "Agente de ventas no encontrado", type = "error" });
                }

                agente.Estado = false;
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Agente de ventas desactivado exitosamente", type = "success" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al desactivar agente de ventas: {ex.Message}", type = "error" });
            }
        }
    }
}
