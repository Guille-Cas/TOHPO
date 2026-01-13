$(document).ready(function () {
    let tabla;

    // Inicializar DataTable
    function inicializarTabla() {
        tabla = $('#tablaImpuestos').DataTable({
            ajax: {
                url: '/Configuracion/Impuestos/Index?handler=Impuestos',
                dataSrc: ''
            },
            columns: [
                { data: 'id' },
                { data: 'descripcion' },
                { 
                    data: 'porcentaje',
                    render: function(data) {
                        return `${data}%`;
                    }
                },
                { 
                    data: 'estado',
                    render: function(data) {
                        return data ? 
                            '<span class="badge bg-success">Activo</span>' : 
                            '<span class="badge bg-danger">Inactivo</span>';
                    }
                },
                {
                    data: 'id',
                    render: function (data, type, row) {
                        const editarBtn = `<button class="btn btn-warning btn-sm btnEditar" data-id="${data}">Editar</button>`;
                        
                        let accionBtn = '';
                        if (row.estado) {
                            accionBtn = `<button class="btn btn-danger btn-sm btnEliminar" data-id="${data}">Eliminar</button>`;
                        } else {
                            accionBtn = `<button class="btn btn-success btn-sm btnActivar" data-id="${data}">Activar</button>`;
                        }
                        
                        return `
                            <div class="d-flex justify-content-center gap-2">
                                ${editarBtn}
                                ${accionBtn}
                            </div>
                        `;
                    }
                }
            ],
            columnDefs: [
                {
                    targets: "_all",
                    className: "align-middle"
                },
                {
                    targets: [0, 1],
                    className: "text-start"
                },
                {
                    targets: [2, 3, 4],
                    className: "text-center"
                }
            ],
            language: {
                emptyTable: "No hay impuestos registrados",
                loadingRecords: "Cargando...",
                search: "Buscar:",
                lengthMenu: "Mostrar _MENU_ registros",
                info: "Mostrando _START_ a _END_ de _TOTAL_ impuestos",
                infoEmpty: "Mostrando 0 impuestos",
                paginate: {
                    first: "Primero",
                    last: "Último",
                    next: "Siguiente",
                    previous: "Anterior"
                }
            }
        });
    }

    // Inicializar tabla al cargar la página
    inicializarTabla();

    // Manejar eliminación de impuesto
    $('#tablaImpuestos').on('click', '.btnEliminar', function () {
        const id = $(this).data('id');
        const fila = $(this).closest('tr');
        const descripcion = tabla.row(fila).data().descripcion;
        
        confirmDelete('impuesto', function() {
            eliminarImpuesto(id);
        }, descripcion);
    });

    // Manejar edición de impuesto
    $('#tablaImpuestos').on('click', '.btnEditar', function () {
        const id = $(this).data('id');
        window.location.href = `/Configuracion/Impuestos/Upsert/${id}`;
    });

    // Manejar activación de impuesto
    $('#tablaImpuestos').on('click', '.btnActivar', function () {
        const id = $(this).data('id');
        const fila = $(this).closest('tr');
        const descripcion = tabla.row(fila).data().descripcion;
        
        confirmAction(`¿Desea activar el impuesto "${descripcion}"?`, function() {
            activarImpuesto(id);
        }, 'Confirmar activación');
    });

    // Función para eliminar impuesto
    async function eliminarImpuesto(id) {
        try {
            const { result, handleResult } = await ajax.post(
                '/Configuracion/Impuestos/Index?handler=Eliminar',
                { id: id }
            );

            if (handleResult.action === 'showDeactivationDialog') {
                feedback.showDeactivationDialog(
                    handleResult.response.message,
                    () => desactivarImpuesto(id),
                    'impuesto'
                );
            } else if (result.success) {
                tabla.ajax.reload(null, false);
            }
        } catch (error) {
            console.error('Error al eliminar impuesto:', error);
        }
    }

    // Función para desactivar impuesto
    async function desactivarImpuesto(id) {
        try {
            const { result } = await ajax.post(
                '/Configuracion/Impuestos/Index?handler=Desactivar',
                { id: id }
            );

            if (result.success) {
                tabla.ajax.reload(null, false);
            }
        } catch (error) {
            console.error('Error al desactivar impuesto:', error);
        }
    }

    // Función para activar impuesto
    async function activarImpuesto(id) {
        try {
            const { result } = await ajax.post(
                '/Configuracion/Impuestos/Index?handler=Editar',
                { id: id, descripcion: '', porcentaje: 0, estado: true }
            );

            if (result.success) {
                tabla.ajax.reload(null, false);
            }
        } catch (error) {
            console.error('Error al activar impuesto:', error);
        }
    }
});