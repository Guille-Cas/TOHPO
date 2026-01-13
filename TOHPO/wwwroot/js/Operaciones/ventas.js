function verDetalle(ventaId) {
    $('#detalleVentaModal').modal('show');
    
    $.get(`/Operaciones/Ventas/Index?handler=DetalleVenta&id=${ventaId}`)
        .done(function(data) {
            if (data.success) {
                const venta = data.venta;
                let html = `
                    <div class="row">
                        <div class="col-md-6">
                            <h6><strong>Información General</strong></h6>
                            <p><strong>ID:</strong> #${venta.id}</p>
                            <p><strong>Fecha:</strong> ${venta.fecha}</p>
                            <p><strong>Hora:</strong> ${venta.hora}</p>
                            <p><strong>Cliente:</strong> ${venta.cliente}</p>
                            ${venta.concepto ? `<p><strong>Concepto:</strong> ${venta.concepto}</p>` : ''}
                        </div>
                        <div class="col-md-6">
                            <h6><strong>Totales</strong></h6>
                            <p><strong>Subtotal:</strong> ₡${venta.costoTotalGravado.toFixed(2)}</p>
                            <p><strong>IVA:</strong> ₡${venta.iva.toFixed(2)}</p>
                            <p><strong>Total:</strong> <span class="text-success">₡${venta.total.toFixed(2)}</span></p>
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
                                    <th>P. Unit.</th>
                                    <th>Desc.</th>
                                    <th>IVA</th>
                                    <th>Subtotal</th>
                                </tr>
                            </thead>
                            <tbody>`;
                
                venta.productos.forEach(producto => {
                    html += `
                        <tr>
                            <td>${producto.producto}</td>
                            <td>${producto.cantidad}</td>
                            <td>₡${producto.precioUnitario.toFixed(2)}</td>
                            <td>₡${producto.montoDescuento.toFixed(2)}</td>
                            <td>₡${producto.montoImpuesto.toFixed(2)}</td>
                            <td>₡${producto.subtotal.toFixed(2)}</td>
                        </tr>`;
                });
                
                html += `
                            </tbody>
                        </table>
                    </div>`;
                
                $('#detalleVentaContent').html(html);
            } else {
                $('#detalleVentaContent').html(`
                    <div class="alert alert-danger">
                        ${data.message}
                    </div>
                `);
            }
        })
        .fail(function() {
            $('#detalleVentaContent').html(`
                <div class="alert alert-danger">
                    Error al cargar el detalle de la venta
                </div>
            `);
        });
}

function confirmarEliminar(ventaId, cliente) {
    if (confirm(`¿Está seguro de que desea eliminar la venta del cliente "${cliente}"?\n\nEsta acción no se puede deshacer.`)) {
        window.location.href = `/Operaciones/Ventas/Index?handler=Eliminar&id=${ventaId}`;
    }
}

function imprimirDetalle() {
    const printContent = document.getElementById('detalleVentaContent').innerHTML;
    const originalContent = document.body.innerHTML;
    
    document.body.innerHTML = `
        <div style="padding: 20px;">
            <h2 style="text-align: center;">Detalle de Venta</h2>
            ${printContent}
        </div>
    `;
    
    window.print();
    document.body.innerHTML = originalContent;
    location.reload();
}

// Inicializar DataTable si hay datos
$(document).ready(function() {
    if ($('#ventasTable tbody tr').length > 1 || ($('#ventasTable tbody tr').length === 1 && !$('#ventasTable tbody tr').first().find('td[colspan]').length)) {
        $('#ventasTable').DataTable({
            language: {
                url: '//cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json'
            },
            order: [[0, 'desc']],
            pageLength: 25,
            responsive: true
        });
    }

    // Similar implementación para ventas
    const buscadorProducto = document.getElementById('inputBuscarProducto');
    if (buscadorProducto) {
        buscadorProducto.focus();
        
        buscadorProducto.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                document.getElementById('btnBuscarProducto').click();
            }
        });

        // Búsqueda automática por código de barras
        let timeoutId = null;
        buscadorProducto.addEventListener('input', function(e) {
            const inputValue = e.target.value;
            
            if (inputValue.length >= 8) {
                clearTimeout(timeoutId);
                timeoutId = setTimeout(() => {
                    buscarProductoPorCodigoVenta(inputValue);
                }, 100);
            }
        });
    }

    function buscarProductoPorCodigoVenta(codigo) {
        fetch(`/api/Productos/BuscarPorCodigo?codigo=${encodeURIComponent(codigo)}`)
            .then(response => response.json())
            .then(data => {
                if (data.success && data.producto) {
                    document.getElementById('inputBuscarProducto').value = data.producto.descripcion;
                    document.getElementById('inputPrecioUnitario').value = data.producto.precioVenta || 0;
                    document.getElementById('hiddenCodigoProducto').value = data.producto.codigoReferencia;
                    
                    // Verificar stock disponible
                    if (data.producto.stock <= 0) {
                        Swal.fire({
                            icon: 'warning',
                            title: 'Sin Stock',
                            text: 'Este producto no tiene stock disponible'
                        });
                        return;
                    }
                    
                    document.getElementById('inputCantidad').focus();
                    document.getElementById('inputCantidad').select();
                }
            })
            .catch(error => {
                console.error('Error buscando producto:', error);
            });
    }

    // Atajos de teclado para ventas
    document.addEventListener('keydown', function(e) {
        if (e.key === 'F2') {
            e.preventDefault();
            document.getElementById('inputBuscarProducto').focus();
            document.getElementById('inputBuscarProducto').select();
        }
        
        if (e.key === 'F3') {
            e.preventDefault();
            document.getElementById('btnAgregarProducto').click();
        }
        
        if (e.key === 'F4') { // Finalizar venta
            e.preventDefault();
            document.getElementById('btnGuardarVenta')?.click();
        }
    });
});