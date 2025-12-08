$(document).ready(function () {
    let tabla;

    // Inicializar DataTable
    function inicializarTabla() {
        tabla = $('#tablaAgentesVentas').DataTable({
            ajax: {
                url: '/Configuracion/Agentes_ventas/Index?handler=AgentesVentas',
                dataSrc: ''
            },
            columns: [
                { data: 'id' },
                { data: 'nombre' },
                { data: 'telefono' },
                { data: 'correo_Electronico' },
                { data: 'proveedor' },
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
                    targets: [0, 1, 2, 3, 4],
                    className: "text-start"
                },
                {
                    targets: [5, 6],
                    className: "text-center"
                }
            ],
            language: {
                emptyTable: "No hay agentes de ventas registrados",
                loadingRecords: "Cargando...",
                search: "Buscar:",
                lengthMenu: "Mostrar _MENU_ registros",
                info: "Mostrando _START_ a _END_ de _TOTAL_ agentes",
                infoEmpty: "Mostrando 0 agentes",
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

    // Manejar eliminación de agente de ventas
    $('#tablaAgentesVentas').on('click', '.btnEliminar', function () {
        const id = $(this).data('id');
        const fila = $(this).closest('tr');
        const nombre = tabla.row(fila).data().nombre;
        
        confirmDelete('agente de ventas', function() {
            eliminarAgenteVentas(id);
        }, nombre);
    });

    // Manejar edición de agente de ventas
    $('#tablaAgentesVentas').on('click', '.btnEditar', function () {
        const id = $(this).data('id');
        window.location.href = `/Configuracion/Agentes_ventas/Upsert/${id}`;
    });

    // Manejar activación de agente de ventas
    $('#tablaAgentesVentas').on('click', '.btnActivar', function () {
        const id = $(this).data('id');
        const fila = $(this).closest('tr');
        const nombre = tabla.row(fila).data().nombre;
        
        confirmAction(`¿Desea activar el agente de ventas "${nombre}"?`, function() {
            activarAgenteVentas(id);
        }, 'Confirmar activación');
    });

    // Función para eliminar agente de ventas
    async function eliminarAgenteVentas(id) {
        try {
            const { result, handleResult } = await ajax.post(
                '/Configuracion/Agentes_ventas/Index?handler=Eliminar',
                { id: id }
            );

            if (handleResult.action === 'showDeactivationDialog') {
                feedback.showDeactivationDialog(
                    handleResult.response.message,
                    () => desactivarAgenteVentas(id),
                    'agente de ventas'
                );
            } else if (result.success) {
                tabla.ajax.reload(null, false);
            }
        } catch (error) {
            console.error('Error al eliminar agente de ventas:', error);
        }
    }

    // Función para desactivar agente de ventas
    async function desactivarAgenteVentas(id) {
        try {
            const { result } = await ajax.post(
                '/Configuracion/Agentes_ventas/Index?handler=Desactivar',
                { id: id }
            );

            if (result.success) {
                tabla.ajax.reload(null, false);
            }
        } catch (error) {
            console.error('Error al desactivar agente de ventas:', error);
        }
    }

    // Función para activar agente de ventas
    async function activarAgenteVentas(id) {
        try {
            const { result } = await ajax.post(
                '/Configuracion/Agentes_ventas/Index?handler=Editar',
                { id: id, nombre: '', telefono: '', correo_Electronico: '', id_Proveedor: null, estado: true }
            );

            if (result.success) {
                tabla.ajax.reload(null, false);
            }
        } catch (error) {
            console.error('Error al activar agente de ventas:', error);
        }
    }
});
