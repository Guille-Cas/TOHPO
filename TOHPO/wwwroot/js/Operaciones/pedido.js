$(document).ready(function() {
    try {
        // Verificar si hay datos en la tabla
        var table = $('#pedidosTable');
        var hasData = table.find('tbody tr').length > 0 && 
                     !table.find('tbody tr').first().find('td[colspan]').length;
        
        if (hasData) {
            $('#pedidosTable').DataTable({
                "language": {
                    "url": "//cdn.datatables.net/plug-ins/1.13.4/i18n/es-ES.json"
                },
                "order": [[ 0, "desc" ]],
                "pageLength": 25,
                "responsive": true,
                "columnDefs": [
                    { "orderable": false, "targets": 9 } // Columna de acciones no ordenable
                ]
            });
        } else {
            console.log('No hay datos para inicializar DataTables');
        }
    } catch (error) {
        console.error('Error inicializando DataTables:', error);
    }
});