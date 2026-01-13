// Funciones para la gestión de compras

function verDetalle(compraId) {
    $('#detalleCompraModal').modal('show');
    
    $.get(`/Operaciones/Compras/Index?handler=DetalleCompra&id=${compraId}`)
        .done(function(data) {
            if (data.success) {
                const compra = data.compra;
                let html = `
                    <div class="row">
                        <div class="col-md-6">
                            <h6><strong>Información General</strong></h6>
                            <p><strong>ID:</strong> #${compra.id}</p>
                            <p><strong>Fecha:</strong> ${compra.fecha}</p>
                            <p><strong>Hora:</strong> ${compra.hora}</p>
                            <p><strong>Proveedor:</strong> ${compra.proveedor}</p>
                            ${compra.concepto ? `<p><strong>Concepto:</strong> ${compra.concepto}</p>` : ''}
                        </div>
                        <div class="col-md-6">
                            <h6><strong>Totales</strong></h6>
                            <p><strong>Subtotal:</strong> ₡${compra.costoTotalGravado.toFixed(2)}</p>
                            <p><strong>IVA:</strong> ₡${compra.iva.toFixed(2)}</p>
                            <p><strong>Total:</strong> <span class="text-success">₡${compra.total.toFixed(2)}</span></p>
                        </div>
                    </div>
                    <hr>
                    <h6><strong>Productos</strong></h6>
                    <div class="table-responsive">
                        <table class="table table-sm">
                            <thead>
                                <tr>
                                    <th>Producto</th>
                                    <th>Cant.</th>
                                    <th>Costo Unit.</th>
                                    <th>Desc.</th>
                                    <th>IVA</th>
                                    <th>Subtotal</th>
                                </tr>
                            </thead>
                            <tbody>`;
        
                compra.productos.forEach(producto => {
                    html += `
                        <tr>
                            <td>${producto.producto}</td>
                            <td>${producto.cantidad}</td>
                            <td>₡${producto.costoUnitario.toFixed(2)}</td>
                            <td>₡${producto.montoDescuento.toFixed(2)}</td>
                            <td>₡${producto.montoImpuesto.toFixed(2)}</td>
                            <td>₡${producto.subtotal.toFixed(2)}</td>
                        </tr>`;
                });
        
                html += `
                            </tbody>
                        </table>
                    </div>`;
                
                $('#detalleCompraContent').html(html);
            } else {
                $('#detalleCompraContent').html(`
                    <div class="alert alert-danger">
                        ${data.message}
                    </div>
                `);
            }
        })
        .fail(function() {
            $('#detalleCompraContent').html(`
                <div class="alert alert-danger">
                    Error al cargar el detalle de la compra
                </div>
            `);
        });
}

function confirmarEliminar(compraId, proveedor) {
    if (confirm(`¿Está seguro de que desea eliminar la compra del proveedor "${proveedor}"?\n\nEsta acción no se puede deshacer.`)) {
        window.location.href = `/Operaciones/Compras/Index?handler=Eliminar&id=${compraId}`;
    }
}

function imprimirDetalle() {
    const printContent = document.getElementById('detalleCompraContent').innerHTML;
    const originalContent = document.body.innerHTML;
    
    document.body.innerHTML = `
        <div style="padding: 20px;">
            <h2 style="text-align: center;">Detalle de Compra</h2>
            ${printContent}
        </div>
    `;
    
    window.print();
    document.body.innerHTML = originalContent;
    location.reload();
}

// Inicializar DataTable cuando el documento esté listo
$(document).ready(function() {
    if ($('#comprasTable tbody tr').length > 1 || ($('#comprasTable tbody tr').length === 1 && !$('#comprasTable tbody tr').first().find('td[colspan]').length)) {
        $('#comprasTable').DataTable({
            language: {
                url: '//cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json'
            },
            order: [[0, 'desc']],
            pageLength: 25,
            responsive: true
        });
    }
});

document.addEventListener('DOMContentLoaded', function() {
    // Enfocar automáticamente el buscador de productos al cargar la página
    const buscadorProducto = document.getElementById('inputBuscarProducto');
    if (buscadorProducto) {
        buscadorProducto.focus();
        
        // Permitir búsqueda con Enter
        buscadorProducto.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                document.getElementById('btnBuscarProducto').click();
            }
        });

        // Búsqueda por código de barras (lectura inmediata)
        let codigoBuffer = '';
        let timeoutId = null;

        buscadorProducto.addEventListener('input', function(e) {
            const inputValue = e.target.value;
            
            // Si el input tiene más de 8 caracteres, probablemente es un código de barras
            if (inputValue.length >= 8) {
                clearTimeout(timeoutId);
                timeoutId = setTimeout(() => {
                    buscarProductoPorCodigo(inputValue);
                }, 100); // Pequeño delay para códigos de barras
            }
        });
    }

    // Función para buscar producto por código automáticamente
    function buscarProductoPorCodigo(codigo) {
        fetch(`/api/Productos/BuscarPorCodigo?codigo=${encodeURIComponent(codigo)}`)
            .then(response => response.json())
            .then(data => {
                if (data.success && data.producto) {
                    // Llenar automáticamente los campos
                    document.getElementById('inputBuscarProducto').value = data.producto.descripcion;
                    document.getElementById('inputPrecioUnitario').value = data.producto.precioCompra || 0;
                    document.getElementById('hiddenCodigoProducto').value = data.producto.codigoReferencia;
                    
                    // Enfocar el campo de cantidad
                    document.getElementById('inputCantidad').focus();
                    document.getElementById('inputCantidad').select();
                }
            })
            .catch(error => {
                console.error('Error buscando producto:', error);
            });
    }

    // Navegación con Tab optimizada
    document.addEventListener('keydown', function(e) {
        if (e.key === 'F2') { // Tecla F2 para volver al buscador
            e.preventDefault();
            document.getElementById('inputBuscarProducto').focus();
            document.getElementById('inputBuscarProducto').select();
        }
        
        if (e.key === 'F3') { // Tecla F3 para agregar producto rápidamente
            e.preventDefault();
            document.getElementById('btnAgregarProducto').click();
        }
    });
});