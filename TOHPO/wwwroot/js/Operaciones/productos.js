$(document).ready(function() {
    $('#productosTable').DataTable({
        "language": {
            "decimal": "",
            "emptyTable": "No hay productos disponibles",
            "info": "Mostrando _START_ a _END_ de _TOTAL_ registros",
            "infoEmpty": "Mostrando 0 a 0 de 0 registros",
            "infoFiltered": "(filtrado de _MAX_ registros totales)",
            "infoPostFix": "",
            "thousands": ",",
            "lengthMenu": "Mostrar _MENU_ registros",
            "loadingRecords": "Cargando...",
            "processing": "Procesando...",
            "search": "Buscar:",
            "zeroRecords": "No se encontraron registros coincidentes",
            "paginate": {
                "first": "Primero",
                "last": "Último",
                "next": "Siguiente",
                "previous": "Anterior"
            },
            "aria": {
                "sortAscending": ": activar para ordenar la columna de manera ascendente",
                "sortDescending": ": activar para ordenar la columna de manera descendente"
            }
        },
        "order": [[1, "asc"]], // Ordenar por descripción
        "columnDefs": [
            { "orderable": false, "targets": 8 } // Desactivar ordenamiento en columna de acciones
        ],
        "pageLength": 10,
        "responsive": true,
        "dom": 'Bfrtip',
        "buttons": [
            {
                extend: 'excelHtml5',
                text: 'Excel',
                className: 'btn btn-success btn-sm',
                title: 'Listado de Productos'
            },
            {
                extend: 'pdfHtml5',
                text: 'PDF',
                className: 'btn btn-danger btn-sm',
                title: 'Listado de Productos',
                orientation: 'landscape',
                pageSize: 'A4'
            }
        ]
    });

    // Mostrar tooltips
    $('[title]').tooltip();
});

function showDetails(codigoReferencia) {
    // Buscar el producto en la tabla
    const productos = window.productosData || [];
    const producto = productos.find(p => p.codigoReferencia === codigoReferencia);
    
    if (producto) {
        let html = `
            <div class="row">
                <div class="col-md-6">
                    <h6>Información Básica</h6>
                    <table class="table table-sm">
                        <tr><td><strong>Código:</strong></td><td>${producto.codigoReferencia}</td></tr>
                        <tr><td><strong>Código Barra:</strong></td><td>${producto.codigo_Barra || 'N/A'}</td></tr>
                        <tr><td><strong>Descripción:</strong></td><td>${producto.descripcion}</td></tr>
                        <tr><td><strong>Unidad Medida:</strong></td><td>${getUnidadMedidaText(producto.unidad_Medida)}</td></tr>
                        <tr><td><strong>Tiempo de Vida:</strong></td><td>${producto.tiempo_De_Vida} días</td></tr>
                    </table>
                </div>
                <div class="col-md-6">
                    <h6>Clasificación</h6>
                    <table class="table table-sm">
                        <tr><td><strong>Categoría:</strong></td><td>${producto.categoria?.descripcion || 'Sin categoría'}</td></tr>
                        <tr><td><strong>Impuesto:</strong></td><td>${producto.impuesto ? producto.impuesto.descripcion + ' (' + producto.impuesto.porcentaje + '%)' : 'Sin impuesto'}</td></tr>
                        <tr><td><strong>Presentación:</strong></td><td>${producto.presentacion?.descripcion || 'Sin presentación'}</td></tr>
                        <tr><td><strong>Materia Prima:</strong></td><td>${producto.materia_Prima?.descripcion || 'N/A'}</td></tr>
                    </table>
                </div>
            </div>
            <div class="row mt-3">
                <div class="col-12">
                    <h6>Características</h6>
                    <div class="d-flex gap-2">
                        ${producto.es_Materia_Prima ? '<span class="badge bg-success">Es Materia Prima</span>' : ''}
                        ${producto.es_De_Terceros ? '<span class="badge bg-warning text-dark">Es de Terceros</span>' : ''}
                        ${producto.estado ? '<span class="badge bg-success">Activo</span>' : '<span class="badge bg-secondary">Inactivo</span>'}
                    </div>
                </div>
            </div>
        `;
        
        $('#detalleContenido').html(html);
    }
}

function getUnidadMedidaText(unidadMedida) {
    const unidades = {
        0: 'Unidad',
        1: 'Kilogramo',
        2: 'Gramo',
        3: 'Litro',
        4: 'Mililitro',
        5: 'Metro',
        6: 'Centímetro',
        7: 'Caja',
        8: 'Paquete'
    };
    return unidades[unidadMedida] || 'Desconocido';
}

function confirmarEliminar(codigoReferencia, descripcion) {
    if (confirm(`¿Está seguro de que desea eliminar el producto "${descripcion}"?\n\nEsta acción no se puede deshacer.`)) {
        // Aquí puedes implementar la lógica de eliminación
        // Por ejemplo, hacer una petición POST al servidor
        window.location.href = `?handler=Eliminar&id=${codigoReferencia}`;
    }
}