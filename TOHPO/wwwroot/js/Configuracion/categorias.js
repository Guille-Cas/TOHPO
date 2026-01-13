$(document).ready(function () {
    let tabla;

    // Inicializar DataTable
    function inicializarTabla() {
        tabla = $('#tablaCategorias').DataTable({
            ajax: {
                url: '/Configuracion/Categorias/Index?handler=Categorias',
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
                emptyTable: "No hay categorías registradas",
                loadingRecords: "Cargando...",
                search: "Buscar:",
                lengthMenu: "Mostrar _MENU_ registros",
                info: "Mostrando _START_ a _END_ de _TOTAL_ categorías",
                infoEmpty: "Mostrando 0 categorías",
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

    // Manejar eliminación de categoría
    $('#tablaCategorias').on('click', '.btnEliminar', function () {
        const id = $(this).data('id');
        const fila = $(this).closest('tr');
        const descripcion = tabla.row(fila).data().descripcion;
        
        confirmDelete('categoría', function() {
            eliminarCategoria(id);
        }, descripcion);
    });

    // Manejar edición de categoría
    $('#tablaCategorias').on('click', '.btnEditar', function () {
        const id = $(this).data('id');
        window.location.href = `/Configuracion/Categorias/Upsert/${id}`;
    });

    // Manejar activación de categoría
    $('#tablaCategorias').on('click', '.btnActivar', function () {
        const id = $(this).data('id');
        const fila = $(this).closest('tr');
        const descripcion = tabla.row(fila).data().descripcion;
        
        confirmAction(`¿Desea activar la categoría "${descripcion}"?`, function() {
            activarCategoria(id);
        }, 'Confirmar activación');
    });

    // Función para eliminar categoría
    async function eliminarCategoria(id) {
        try {
            const { result, handleResult } = await ajax.post(
                '/Configuracion/Categorias/Index?handler=Eliminar',
                { id: id }
            );

            if (handleResult.action === 'showDeactivationDialog') {
                feedback.showDeactivationDialog(
                    handleResult.response.message,
                    () => desactivarCategoria(id),
                    'categoría'
                );
            } else if (result.success) {
                tabla.ajax.reload(null, false);
            }
        } catch (error) {
            console.error('Error al eliminar categoría:', error);
        }
    }

    // Función para desactivar categoría
    async function desactivarCategoria(id) {
        try {
            const { result } = await ajax.post(
                '/Configuracion/Categorias/Index?handler=Desactivar',
                { id: id }
            );

            if (result.success) {
                tabla.ajax.reload(null, false);
            }
        } catch (error) {
            console.error('Error al desactivar categoría:', error);
        }
    }

    // Función para activar categoría
    async function activarCategoria(id) {
        try {
            const { result } = await ajax.post(
                '/Configuracion/Categorias/Index?handler=Editar',
                { id: id, descripcion: '', estado: true }
            );

            if (result.success) {
                tabla.ajax.reload(null, false);
            }
        } catch (error) {
            console.error('Error al activar categoría:', error);
        }
    }
});