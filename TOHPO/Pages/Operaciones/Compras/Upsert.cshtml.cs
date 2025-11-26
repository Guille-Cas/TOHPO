using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using TOHPO.Data;
using TOHPO.Models;

namespace TOHPO.Pages.Operaciones.Compras
{
    public class UpsertModel : PageModel
    {
        private readonly AppDbContext _context;

        public UpsertModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Compra Compra { get; set; } = default!;

        [BindProperty]
        public string ProductosJson { get; set; } = string.Empty;

        public SelectList ProveedoresSelectList { get; set; } = default!;
        public List<ProductoViewModel> Productos { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            await CargarDatosComunes();

            if (id == null || id <= 0)
            {
                Compra = new Compra
                {
                    Fecha = DateTime.Now,
                    Estado = false,
                    Total = 0,
                    Iva = 0,
                    Gran_Total = 0
                };
                return Page();
            }

            try
            {
                var compra = await _context.Compra
                    .Include(c => c.Proveedor)
                    .Include(c => c.Compra_Detalles)
                        .ThenInclude(cd => cd.Producto)
                            .ThenInclude(p => p.Impuesto)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (compra == null)
                {
                    TempData["Error"] = "La compra no fue encontrada.";
                    return RedirectToPage("./Index");
                }

                if (compra.Estado)
                {
                    TempData["Warning"] = "Esta compra ya ha sido procesada y solo puede visualizarse.";
                }

                // Debug: Verificar que los datos se cargaron correctamente
                System.Diagnostics.Debug.WriteLine($"Compra cargada - ID: {compra.Id}, Detalles: {compra.Compra_Detalles?.Count ?? 0}");
                
                foreach (var detalle in compra.Compra_Detalles ?? new List<Compra_Detalle>())
                {
                    System.Diagnostics.Debug.WriteLine($"Detalle: {detalle.Codigo_Producto}, Producto: {detalle.Producto?.Descripcion ?? "NULL"}");
                }

                Compra = compra;
                return Page();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando compra: {ex.Message}");
                TempData["Error"] = $"Error al cargar la compra: {ex.Message}";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Debug del JSON recibido
            if (!string.IsNullOrEmpty(ProductosJson))
            {
                System.Diagnostics.Debug.WriteLine($"ProductosJson recibido: {ProductosJson}");
            }

            // Configurar opciones para deserialización con camelCase
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            // Procesar productos desde JSON
            var productos = new List<ProductoCompra>();
            if (!string.IsNullOrEmpty(ProductosJson))
            {
                try
                {
                    productos = JsonSerializer.Deserialize<List<ProductoCompra>>(ProductosJson, options) ?? new List<ProductoCompra>();
                    System.Diagnostics.Debug.WriteLine($"Productos deserializados: {productos.Count}");
                    
                    foreach (var prod in productos)
                    {
                        System.Diagnostics.Debug.WriteLine($"Producto: {prod.Codigo}, Cantidad: {prod.Cantidad}, Costo: {prod.CostoUnitario}");
                    }
                }
                catch (JsonException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deserialización: {ex.Message}");
                    ModelState.AddModelError(string.Empty, $"Error al procesar los productos: {ex.Message}");
                    await CargarDatosComunes();
                    return Page();
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ProductosJson está vacío");
            }

            // Validaciones básicas
            if (productos.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Debe agregar al menos un producto a la compra.");
            }

            if (Compra.Id_Proveedor <= 0)
            {
                ModelState.AddModelError("Compra.Id_Proveedor", "Debe seleccionar un proveedor.");
            }

            // Validar que exista el proveedor
            if (Compra.Id_Proveedor > 0)
            {
                var proveedorExiste = await _context.Proveedor
                    .AnyAsync(p => p.Id == Compra.Id_Proveedor);

                if (!proveedorExiste)
                {
                    ModelState.AddModelError("Compra.Id_Proveedor", "El proveedor seleccionado no es válido o está inactivo.");
                }
            }

            // Validar productos y verificar que tengan impuesto asociado
            var productosValidos = new List<ProductoCompra>();
            foreach (var prod in productos)
            {
                if (string.IsNullOrEmpty(prod.Codigo))
                {
                    ModelState.AddModelError(string.Empty, "Todos los productos deben tener un código válido.");
                    continue;
                }

                var producto = await _context.Producto
                    .Include(p => p.Impuesto)
                    .FirstOrDefaultAsync(p => p.CodigoReferencia == prod.Codigo && p.Estado);

                if (producto == null)
                {
                    ModelState.AddModelError(string.Empty, $"El producto {prod.Codigo} no existe o está inactivo.");
                    continue;
                }

                if (producto.Impuesto == null)
                {
                    ModelState.AddModelError(string.Empty, $"El producto {prod.Codigo} no tiene un impuesto asociado.");
                    continue;
                }

                if (prod.Cantidad <= 0)
                {
                    ModelState.AddModelError(string.Empty, $"La cantidad del producto {prod.Codigo} debe ser mayor a 0.");
                    continue;
                }

                if (prod.CostoUnitario <= 0)
                {
                    ModelState.AddModelError(string.Empty, $"El costo unitario del producto {prod.Codigo} debe ser mayor a 0.");
                    continue;
                }

                if (prod.PorcentajeDescuento < 0 || prod.PorcentajeDescuento > 100)
                {
                    ModelState.AddModelError(string.Empty, $"El porcentaje de descuento del producto {prod.Codigo} debe estar entre 0 y 100.");
                    continue;
                }

                // Si pasa todas las validaciones, agregarlo a la lista de productos válidos
                prod.PorcentajeImpuesto = producto.Impuesto.Porcentaje;
                productosValidos.Add(prod);
            }

            System.Diagnostics.Debug.WriteLine($"Productos válidos: {productosValidos.Count}");

            // Recalcular totales con productos válidos ANTES de la validación del modelo
            if (productosValidos.Count > 0)
            {
                decimal subtotal = 0;
                decimal descuentoTotal = 0;
                decimal ivaTotal = 0;

                foreach (var prod in productosValidos)
                {
                    decimal subtotalProducto = prod.CostoUnitario * prod.Cantidad;
                    decimal subtotalConDescuento = subtotalProducto - prod.MontoDescuento;
                    
                    subtotal += subtotalProducto;
                    descuentoTotal += prod.MontoDescuento;
                    
                    // Calcular IVA basado en el porcentaje de impuesto del producto
                    decimal ivaProducto = subtotalConDescuento * (prod.PorcentajeImpuesto / 100);
                    ivaTotal += ivaProducto;
                }

                decimal totalGravado = subtotal - descuentoTotal;
                decimal granTotal = totalGravado + ivaTotal;

                // Asignar los valores calculados a la compra
                Compra.Total = totalGravado;
                Compra.Iva = ivaTotal;
                Compra.Gran_Total = granTotal;

                // CRUCIAL: Limpiar los errores de validación de estos campos calculados
                ModelState.Remove("Compra.Total");
                ModelState.Remove("Compra.Iva");
                ModelState.Remove("Compra.Gran_Total");

                System.Diagnostics.Debug.WriteLine($"Totales calculados - Total: {totalGravado}, IVA: {ivaTotal}, Gran Total: {granTotal}");
            }
            else if (productos.Count > 0)
            {
                // Si hay productos pero ninguno es válido, establecer totales en 0
                Compra.Total = 0;
                Compra.Iva = 0;
                Compra.Gran_Total = 0;
                
                // Limpiar errores de validación
                ModelState.Remove("Compra.Total");
                ModelState.Remove("Compra.Iva");
                ModelState.Remove("Compra.Gran_Total");
            }

            // Validar campos requeridos del modelo
            if (string.IsNullOrEmpty(Compra.Numero_Factura))
            {
                ModelState.AddModelError("Compra.Numero_Factura", "El número de factura es obligatorio.");
            }

            // Debug: Verificar el estado del ModelState después de limpiar
            System.Diagnostics.Debug.WriteLine($"ModelState.IsValid después de limpiar: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                await CargarDatosComunes();
                return Page();
            }

            try
            {
                if (Compra.Id == 0)
                {
                    // Nueva compra
                    _context.Compra.Add(Compra);
                    await _context.SaveChangesAsync();
                    
                    System.Diagnostics.Debug.WriteLine($"Compra guardada con ID: {Compra.Id}");

                    // Agregar detalles
                    foreach (var prod in productosValidos)
                    {
                        var detalle = new Compra_Detalle
                        {
                            Id_Compra = Compra.Id,
                            Codigo_Producto = prod.Codigo,
                            Cantidad = prod.Cantidad,
                            Costo_Unitario = prod.CostoUnitario,
                            Porcentaje_Descuento = prod.PorcentajeDescuento,
                            Monto_Descuento = prod.MontoDescuento
                        };
                        _context.Compra_Detalle.Add(detalle);
                        System.Diagnostics.Debug.WriteLine($"Detalle agregado: {prod.Codigo}");
                    }

                    TempData["Success"] = "La compra ha sido creada correctamente.";
                }
                else
                {
                    // Editar compra existente
                    var compraExistente = await _context.Compra
                        .Include(c => c.Compra_Detalles)
                        .FirstOrDefaultAsync(c => c.Id == Compra.Id);

                    if (compraExistente == null)
                    {
                        return NotFound();
                    }

                    if (compraExistente.Estado)
                    {
                        TempData["Error"] = "No se puede modificar una compra que ya ha sido procesada.";
                        return RedirectToPage("./Index");
                    }

                    // Actualizar datos de la compra
                    compraExistente.Fecha = Compra.Fecha;
                    compraExistente.Numero_Factura = Compra.Numero_Factura;
                    compraExistente.Id_Proveedor = Compra.Id_Proveedor;
                    compraExistente.Total = Compra.Total;
                    compraExistente.Iva = Compra.Iva;
                    compraExistente.Gran_Total = Compra.Gran_Total;

                    // Eliminar detalles existentes
                    _context.Compra_Detalle.RemoveRange(compraExistente.Compra_Detalles);

                    // Agregar nuevos detalles
                    foreach (var prod in productosValidos)
                    {
                        var detalle = new Compra_Detalle
                        {
                            Id_Compra = Compra.Id,
                            Codigo_Producto = prod.Codigo,
                            Cantidad = prod.Cantidad,
                            Costo_Unitario = prod.CostoUnitario,
                            Porcentaje_Descuento = prod.PorcentajeDescuento,
                            Monto_Descuento = prod.MontoDescuento
                        };
                        _context.Compra_Detalle.Add(detalle);
                    }

                    TempData["Success"] = "La compra ha sido actualizada correctamente.";
                }

                await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine("Cambios guardados en la base de datos");
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al guardar: {ex.Message}");
                ModelState.AddModelError(string.Empty, $"Error al procesar la compra: {ex.Message}");
                await CargarDatosComunes();
                return Page();
            }
        }

        private async Task CargarDatosComunes()
        {
            try
            {
                // Cargar proveedores activos
                var proveedores = await _context.Proveedor
                    .OrderBy(p => p.Nombre)
                    .Select(p => new { p.Id, p.Nombre })
                    .ToListAsync();

                ProveedoresSelectList = new SelectList(proveedores, "Id", "Nombre");

                // Obtener todos los productos activos con su impuesto
                var productosQuery = await _context.Producto
                    .Include(p => p.Impuesto)
                    .Where(p => p.Estado)
                    .OrderBy(p => p.Descripcion)
                    .ToListAsync();

                if (productosQuery.Count == 0)
                {
                    Productos = new List<ProductoViewModel>();
                    return;
                }

                // Obtener los códigos de los productos
                var codigos = productosQuery.Select(p => p.CodigoReferencia).ToList();

                // Obtener los inventarios relacionados
                var inventarios = await _context.Inventario
                    .Where(i => codigos.Contains(i.Codigo_Producto))
                    .ToListAsync();

                // Mapear los productos con su inventario e impuesto
                Productos = productosQuery.Select(p =>
                {
                    var inventario = inventarios.FirstOrDefault(i => i.Codigo_Producto == p.CodigoReferencia);
                    return new ProductoViewModel
                    {
                        Codigo = p.CodigoReferencia,
                        Nombre = p.Descripcion,
                        Precio_Compra = inventario?.Precio_Compra ?? 0,
                        Stock = inventario?.Existencia ?? 0,
                        PorcentajeImpuesto = p.Impuesto?.Porcentaje ?? 0,
                        DescripcionImpuesto = p.Impuesto?.Descripcion ?? "Sin impuesto"
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                // Log del error
                Productos = new List<ProductoViewModel>();
                ProveedoresSelectList = new SelectList(new List<object>(), "Id", "Nombre");
                
                ModelState.AddModelError(string.Empty, $"Error al cargar datos: {ex.Message}");
            }
        }
    }

    public class ProductoCompra
    {
        [JsonPropertyName("codigo")]
        public string Codigo { get; set; } = string.Empty;
        
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;
        
        [JsonPropertyName("cantidad")]
        public int Cantidad { get; set; }
        
        [JsonPropertyName("costoUnitario")]
        public decimal CostoUnitario { get; set; }
        
        [JsonPropertyName("porcentajeDescuento")]
        public decimal PorcentajeDescuento { get; set; }
        
        [JsonPropertyName("montoDescuento")]
        public decimal MontoDescuento { get; set; }
        
        [JsonPropertyName("porcentajeImpuesto")]
        public decimal PorcentajeImpuesto { get; set; }
    }

    public class ProductoViewModel
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio_Compra { get; set; }
        public int Stock { get; set; }
        public decimal PorcentajeImpuesto { get; set; }
        public string DescripcionImpuesto { get; set; } = string.Empty;
    }
}