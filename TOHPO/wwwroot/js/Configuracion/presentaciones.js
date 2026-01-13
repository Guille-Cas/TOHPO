$(document).ready(function () {
    let tabla;

    // Inicializar DataTable
    function inicializarTabla() {
        tabla = $('#tablaPresentaciones').DataTable({
            ajax: {
                url: '/Configuracion/Presentaciones/Index?handler=Presentaciones',
                dataSrc: ''
            },
            columns: [
                { data: 'id' },
                { data: 'cantidad' },
                { data: 'unidad_Medida' },
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
                    targets: [0, 1, 2],
                    className: "text-start"
                },
                {
                    targets: [3, 4],
                    className: "text-center"
                }
            ],
            language: {
                emptyTable: "No hay presentaciones registradas",
                loadingRecords: "Cargando...",
                search: "Buscar:",
                lengthMenu: "Mostrar _MENU_ registros",
                info: "Mostrando _START_ a _END_ de _TOTAL_ presentaciones",
                infoEmpty: "Mostrando 0 presentaciones",
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

    // Manejar eliminación de presentación
    $('#tablaPresentaciones').on('click', '.btnEliminar', function () {
        const id = $(this).data('id');
        const fila = $(this).closest('tr');
        const data = tabla.row(fila).data();
        const presentacion = `${data.cantidad} ${data.unidad_Medida}`;
        
        confirmDelete('presentación', function() {
            eliminarPresentacion(id);
        }, presentacion);
    });

    // Manejar edición de presentación
    $('#tablaPresentaciones').on('click', '.btnEditar', function () {
        const id = $(this).data('id');
        window.location.href = `/Configuracion/Presentaciones/Upsert/${id}`;
    });

    // Manejar activación de presentación
    $('#tablaPresentaciones').on('click', '.btnActivar', function () {
        const id = $(this).data('id');
        const fila = $(this).closest('tr');
        const data = tabla.row(fila).data();
        const presentacion = `${data.cantidad} ${data.unidad_Medida}`;
        
        confirmAction(`¿Desea activar la presentación "${presentacion}"?`, function() {
            activarPresentacion(id);
        }, 'Confirmar activación');
    });

    // Función para eliminar presentación
    async function eliminarPresentacion(id) {
        try {
            const { result, handleResult } = await ajax.post(
                '/Configuracion/Presentaciones/Index?handler=Eliminar',
                { id: id }
            );

            if (handleResult.action === 'showDeactivationDialog') {
                feedback.showDeactivationDialog(
                    handleResult.response.message,
                    () => desactivarPresentacion(id),
                    'presentación'
                );
            } else if (result.success) {
                tabla.ajax.reload(null, false);
            }
        } catch (error) {
            console.error('Error al eliminar presentación:', error);
        }
    }

    // Función para desactivar presentación
    async function desactivarPresentacion(id) {
        try {
            const { result } = await ajax.post(
                '/Configuracion/Presentaciones/Index?handler=Desactivar',
                { id: id }
            );

            if (result.success) {
                tabla.ajax.reload(null, false);
            }
        } catch (error) {
            console.error('Error al desactivar presentación:', error);
        }
    }

    // Función para activar presentación
    async function activarPresentacion(id) {
        try {
            const { result } = await ajax.post(
                '/Configuracion/Presentaciones/Index?handler=Editar',
                { id: id, cantidad: 0, unidad_Medida: '', estado: true }
            );

            if (result.success) {
                tabla.ajax.reload(null, false);
            }
        } catch (error) {
            console.error('Error al activar presentación:', error);
        }
    }
});