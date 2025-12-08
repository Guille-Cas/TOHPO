$(document).ready(function () {
    let tabla;

    // Inicializar DataTable
    function inicializarTabla() {
        tabla = $('#tablaMateriasPrima').DataTable({
            ajax: {
                url: '/Configuracion/Materias_Prima/Index?handler=MateriasPrima',
                dataSrc: ''
            },
            columns: [
                { data: 'id' },
                { data: 'descripcion' },
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
                emptyTable: "No hay materias primas registradas",
                loadingRecords: "Cargando...",
                search: "Buscar:",
                lengthMenu: "Mostrar _MENU_ registros",
                info: "Mostrando _START_ a _END_ de _TOTAL_ materias primas",
                infoEmpty: "Mostrando 0 materias primas",
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

    // Manejar eliminación de materia prima
    $('#tablaMateriasPrima').on('click', '.btnEliminar', function () {
        const id = $(this).data('id');
        const fila = $(this).closest('tr');
        const descripcion = tabla.row(fila).data().descripcion;
        
        confirmDelete('materia prima', function() {
            eliminarMateriaPrima(id);
        }, descripcion);
    });

    // Manejar edición de materia prima
    $('#tablaMateriasPrima').on('click', '.btnEditar', function () {
        const id = $(this).data('id');
        window.location.href = `/Configuracion/Materias_Prima/Upsert/${id}`;
    });

    // Manejar activación de materia prima
    $('#tablaMateriasPrima').on('click', '.btnActivar', function () {
        const id = $(this).data('id');
        const fila = $(this).closest('tr');
        const descripcion = tabla.row(fila).data().descripcion;
        
        confirmAction(`¿Desea activar la materia prima "${descripcion}"?`, function() {
            activarMateriaPrima(id);
        }, 'Confirmar activación');
    });

    // Función para eliminar materia prima
    async function eliminarMateriaPrima(id) {
        try {
            const { result, handleResult } = await ajax.post(
                '/Configuracion/Materias_Prima/Index?handler=Eliminar',
                { id: id }
            );

            if (handleResult.action === 'showDeactivationDialog') {
                feedback.showDeactivationDialog(
                    handleResult.response.message,
                    () => desactivarMateriaPrima(id),
                    'materia prima'
                );
            } else if (result.success) {
                tabla.ajax.reload(null, false);
            }
        } catch (error) {
            console.error('Error al eliminar materia prima:', error);
        }
    }

    // Función para desactivar materia prima
    async function desactivarMateriaPrima(id) {
        try {
            const { result } = await ajax.post(
                '/Configuracion/Materias_Prima/Index?handler=Desactivar',
                { id: id }
            );

            if (result.success) {
                tabla.ajax.reload(null, false);
            }
        } catch (error) {
            console.error('Error al desactivar materia prima:', error);
        }
    }

    // Función para activar materia prima
    async function activarMateriaPrima(id) {
        try {
            const { result } = await ajax.post(
                '/Configuracion/Materias_Prima/Index?handler=Editar',
                { id: id, descripcion: '', unidad_Medida: '', estado: true }
            );

            if (result.success) {
                tabla.ajax.reload(null, false);
            }
        } catch (error) {
            console.error('Error al activar materia prima:', error);
        }
    }
});