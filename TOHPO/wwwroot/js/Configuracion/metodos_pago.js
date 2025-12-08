$(document).ready(function () {
    let tabla;

    // Inicializar DataTable
    function inicializarTabla() {
        tabla = $('#tablaMetodosPago').DataTable({
            ajax: {
                url: '/Configuracion/Metodos_Pago/Index?handler=MetodosPago',
                dataSrc: ''
            },
            columns: [
                { data: 'id' },
                { data: 'descripcion' },
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
                    targets: [2, 3],
                    className: "text-center"
                }
            ],
            language: {
                emptyTable: "No hay métodos de pago registrados",
                loadingRecords: "Cargando...",
                search: "Buscar:",
                lengthMenu: "Mostrar _MENU_ registros",
                info: "Mostrando _START_ a _END_ de _TOTAL_ métodos de pago",
                infoEmpty: "Mostrando 0 métodos de pago",
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

    // Manejar eliminación de método de pago
    $('#tablaMetodosPago').on('click', '.btnEliminar', function () {
        const id = $(this).data('id');
        const fila = $(this).closest('tr');
        const descripcion = tabla.row(fila).data().descripcion;
        
        confirmDelete('método de pago', function() {
            eliminarMetodoPago(id);
        }, descripcion);
    });

    // Manejar edición de método de pago
    $('#tablaMetodosPago').on('click', '.btnEditar', function () {
        const id = $(this).data('id');
        window.location.href = `/Configuracion/Metodos_Pago/Upsert/${id}`;
    });

    // Manejar activación de método de pago
    $('#tablaMetodosPago').on('click', '.btnActivar', function () {
        const id = $(this).data('id');
        const fila = $(this).closest('tr');
        const descripcion = tabla.row(fila).data().descripcion;
        
        confirmAction(`¿Desea activar el método de pago "${descripcion}"?`, function() {
            activarMetodoPago(id);
        }, 'Confirmar activación');
    });

    // Función para eliminar método de pago
    async function eliminarMetodoPago(id) {
        try {
            const { result, handleResult } = await ajax.post(
                '/Configuracion/Metodos_Pago/Index?handler=Eliminar',
                { id: id }
            );

            if (handleResult.action === 'showDeactivationDialog') {
                feedback.showDeactivationDialog(
                    handleResult.response.message,
                    () => desactivarMetodoPago(id),
                    'método de pago'
                );
            } else if (result.success) {
                tabla.ajax.reload(null, false);
            }
        } catch (error) {
            console.error('Error al eliminar método de pago:', error);
        }
    }

    // Función para desactivar método de pago
    async function desactivarMetodoPago(id) {
        try {
            const { result } = await ajax.post(
                '/Configuracion/Metodos_Pago/Index?handler=Desactivar',
                { id: id }
            );

            if (result.success) {
                tabla.ajax.reload(null, false);
            }
        } catch (error) {
            console.error('Error al desactivar método de pago:', error);
        }
    }

    // Función para activar método de pago
    async function activarMetodoPago(id) {
        try {
            const { result } = await ajax.post(
                '/Configuracion/Metodos_Pago/Index?handler=Editar',
                { id: id, descripcion: '', estado: true }
            );

            if (result.success) {
                tabla.ajax.reload(null, false);
            }
        } catch (error) {
            console.error('Error al activar método de pago:', error);
        }
    }
});