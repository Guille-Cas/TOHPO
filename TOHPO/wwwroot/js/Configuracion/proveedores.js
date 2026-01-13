$(document).ready(function () {
    let tabla;

    // Inicializar DataTable
    function inicializarTabla() {
        tabla = $('#tablaProveedores').DataTable({
            ajax: {
                url: '/Configuracion/Proveedores/Index?handler=Proveedores',
                dataSrc: ''
            },
            columns: [
                { data: 'id' },
                { data: 'nombre' },
                { data: 'telefono' },
                { data: 'correo' },
                { data: 'direccion' },
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
                emptyTable: "No hay proveedores registrados",
                loadingRecords: "Cargando...",
                search: "Buscar:",
                lengthMenu: "Mostrar _MENU_ registros",
                info: "Mostrando _START_ a _END_ de _TOTAL_ proveedores",
                infoEmpty: "Mostrando 0 proveedores",
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

    // Manejar eliminación de proveedor
    $('#tablaProveedores').on('click', '.btnEliminar', function () {
        const id = $(this).data('id');
        const fila = $(this).closest('tr');
        const nombre = tabla.row(fila).data().nombre;
        
        confirmDelete('proveedor', function() {
            eliminarProveedor(id);
        }, nombre);
    });

    // Manejar edición de proveedor
    $('#tablaProveedores').on('click', '.btnEditar', function () {
        const id = $(this).data('id');
        window.location.href = `/Configuracion/Proveedores/Upsert/${id}`;
    });

    // Manejar activación de proveedor
    $('#tablaProveedores').on('click', '.btnActivar', function () {
        const id = $(this).data('id');
        const fila = $(this).closest('tr');
        const nombre = tabla.row(fila).data().nombre;
        
        confirmAction(`¿Desea activar el proveedor "${nombre}"?`, function() {
            activarProveedor(id);
        }, 'Confirmar activación');
    });

    // Función para eliminar proveedor
    async function eliminarProveedor(id) {
        try {
            const { result, handleResult } = await ajax.post(
                '/Configuracion/Proveedores/Index?handler=Eliminar',
                { id: id }
            );

            if (handleResult.action === 'showDeactivationDialog') {
                feedback.showDeactivationDialog(
                    handleResult.response.message,
                    () => desactivarProveedor(id),
                    'proveedor'
                );
            } else if (result.success) {
                tabla.ajax.reload(null, false);
            }
        } catch (error) {
            console.error('Error al eliminar proveedor:', error);
        }
    }

    // Función para desactivar proveedor
    async function desactivarProveedor(id) {
        try {
            const { result } = await ajax.post(
                '/Configuracion/Proveedores/Index?handler=Desactivar',
                { id: id }
            );

            if (result.success) {
                tabla.ajax.reload(null, false);
            }
        } catch (error) {
            console.error('Error al desactivar proveedor:', error);
        }
    }

    // Función para activar proveedor
    async function activarProveedor(id) {
        try {
            const { result } = await ajax.post(
                '/Configuracion/Proveedores/Index?handler=Editar',
                { id: id, nombre: '', telefono: '', correo_Electronico: '', direccion: '', estado: true }
            );

            if (result.success) {
                tabla.ajax.reload(null, false);
            }
        } catch (error) {
            console.error('Error al activar proveedor:', error);
        }
    }
});