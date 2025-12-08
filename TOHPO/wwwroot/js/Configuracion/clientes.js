$(document).ready(function () {
    let tabla;
    let modoEdicion = false;
    let clienteEditando = null;

    // Inicializar DataTable
    function inicializarTabla() {
        tabla = $('#tablaClientes').DataTable({
            ajax: {
                url: '/Configuracion/Clientes/Index?handler=Clientes',
                dataSrc: ''
            },
            columns: [
                { data: 'id' },
                { data: 'cedula' },
                { data: 'nombre' },
                { data: 'primer_Apellido' },
                { data: 'segundo_Apellido' },
                { data: 'telefono' },
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
                        const verBtn = `<button class="btn btn-info btn-sm btnVerDetalles" data-id="${data}">Ver</button>`;
                        const editarBtn = `<button class="btn btn-warning btn-sm btnEditar" data-id="${data}">Editar</button>`;
                        
                        let accionesBtn = '';
                        if (row.estado) {
                            accionesBtn = `
                                <button class="btn btn-danger btn-sm btnEliminar" data-id="${data}">Eliminar</button>
                                <button class="btn btn-secondary btn-sm btnDesactivar" data-id="${data}">Desactivar</button>
                            `;
                        } else {
                            accionesBtn = `<button class="btn btn-success btn-sm btnActivar" data-id="${data}">Activar</button>`;
                        }
                        
                        return `
                            <div class="d-flex justify-content-center gap-2 flex-wrap">
                                ${verBtn}
                                ${editarBtn}
                                ${accionesBtn}
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
                    targets: [0, 1, 2, 3, 4, 5],
                    className: "text-start"
                },
                {
                    targets: [6, 7],
                    className: "text-center"
                }
            ],
            language: {
                emptyTable: "No hay clientes registrados en el sistema",
                loadingRecords: "Cargando...",
                search: "Buscar:",
                lengthMenu: "Mostrar _MENU_ registros",
                info: "Mostrando _START_ a _END_ de _TOTAL_ clientes",
                infoEmpty: "Mostrando 0 clientes",
                paginate: {
                    first: "Primero",
                    last: "Último",
                    next: "Siguiente",
                    previous: "Anterior"
                }
            }
        });
    }

    // Función para recargar tabla
    function recargarTabla() {
        tabla.ajax.reload(null, false);
    }

    // Validar formulario de cliente
    function validarFormularioCliente() {
        const nombre = $('#nombre').val().trim();
        const primerApellido = $('#primerApellido').val().trim();
        const segundoApellido = $('#segundoApellido').val().trim();
        const cedula = $('#cedula').val().trim();
        const telefono = $('#telefono').val().trim();

        if (!nombre) {
            feedback.error('El nombre es requerido');
            return false;
        }
        if (!primerApellido) {
            feedback.error('El primer apellido es requerido');
            return false;
        }
        if (!segundoApellido) {
            feedback.error('El segundo apellido es requerido');
            return false;
        }
        if (!cedula) {
            feedback.error('La cédula es requerida');
            return false;
        }
        if (!telefono) {
            feedback.error('El teléfono es requerido');
            return false;
        }

        // Validar formato de teléfono (0000-0000)
        const telefonoRegex = /^\d{4}-\d{4}$/;
        if (!telefonoRegex.test(telefono)) {
            feedback.error('El formato del teléfono debe ser 0000-0000');
            return false;
        }

        return true;
    }

    // Inicializar tabla
    inicializarTabla();

    // Nuevo cliente
    $('#btnNuevoCliente').on('click', function () {
        modoEdicion = false;
        clienteEditando = null;
        $('#modalClienteLabel').text('Nuevo Cliente');
        $('#btnGuardarCliente').text('Crear Cliente');
        $('#formCliente')[0].reset();
        $('#estado').prop('checked', true);
        
        var modal = new bootstrap.Modal(document.getElementById('modalCliente'));
        modal.show();
    });

    // Ver detalles del cliente
    $('#tablaClientes').on('click', '.btnVerDetalles', function () {
        const id = $(this).data('id');
        const fila = tabla.row($(this).parents('tr')).data();
        
        // Llenar el modal con los datos del cliente
        $('#modalId').text(fila.id);
        $('#modalCedula').text(fila.cedula);
        $('#modalNombre').text(fila.nombre);
        $('#modalPrimerApellido').text(fila.primer_Apellido);
        $('#modalSegundoApellido').text(fila.segundo_Apellido);
        $('#modalCorreo').text(fila.correo_Electronico || 'No especificado');
        $('#modalTelefono').text(fila.telefono);
        
        // Mostrar el modal
        const modal = new bootstrap.Modal(document.getElementById('detallesModal'));
        modal.show();
    });

    // Editar cliente - corregido para redirigir a la página Upsert
    $('#tablaClientes').on('click', '.btnEditar', function () {
        const id = $(this).data('id');
        window.location.href = `/Configuracion/Clientes/Upsert?id=${id}`;
    });

    // Guardar cliente
    $('#btnGuardarCliente').on('click', async function () {
        if (!validarFormularioCliente()) {
            return;
        }

        try {
            const formData = new FormData($('#formCliente')[0]);
            
            let url;
            if (modoEdicion) {
                url = '/Configuracion/Clientes/Index?handler=Editar';
                formData.append('Id', clienteEditando);
            } else {
                url = '/Configuracion/Clientes/Index?handler=Crear';
            }

            const { result } = await ajax.post(url, formData, () => {
                $('#modalCliente').modal('hide');
                recargarTabla();
            });

        } catch (error) {
            // Error ya manejado por AjaxHelper
        }
    });

    // Eliminar cliente
    $('#tablaClientes').on('click', '.btnEliminar', function () {
        const id = $(this).data('id');
        const fila = tabla.row($(this).parents('tr')).data();
        const nombreCompleto = `${fila.nombre} ${fila.primer_Apellido} ${fila.segundo_Apellido}`;
        
        feedback.confirmDelete('cliente', () => eliminarCliente(id), nombreCompleto);
    });

    async function eliminarCliente(id) {
        try {
            const { result, handleResult } = await ajax.post('/Configuracion/Clientes/Index?handler=Eliminar', { id });
            
            // Si es un caso de validación que requiere desactivación
            if (handleResult.action === 'showDeactivationDialog') {
                const response = handleResult.response;
                feedback.showDeactivationDialog(response.message, () => desactivarCliente(id), 'cliente');
            } else if (result.success) {
                recargarTabla();
            }
        } catch (error) {
            // Error ya manejado
        }
    }

    // Desactivar cliente
    $('#tablaClientes').on('click', '.btnDesactivar', function () {
        const id = $(this).data('id');
        const fila = tabla.row($(this).parents('tr')).data();
        const nombreCompleto = `${fila.nombre} ${fila.primer_Apellido} ${fila.segundo_Apellido}`;
        
        feedback.confirm(`¿Desea desactivar al cliente "${nombreCompleto}"?`, () => desactivarCliente(id));
    });

    async function desactivarCliente(id) {
        try {
            await ajax.post('/Configuracion/Clientes/Index?handler=Desactivar', { id }, () => {
                recargarTabla();
            });
        } catch (error) {
            // Error ya manejado
        }
    }

    // Activar cliente
    $('#tablaClientes').on('click', '.btnActivar', function () {
        const id = $(this).data('id');
        const fila = tabla.row($(this).parents('tr')).data();
        const nombreCompleto = `${fila.nombre} ${fila.primer_Apellido} ${fila.segundo_Apellido}`;
        
        feedback.confirm(`¿Desea activar al cliente "${nombreCompleto}"?`, () => activarCliente(id));
    });

    async function activarCliente(id) {
        try {
            await ajax.post('/Configuracion/Clientes/Index?handler=Activar', { id }, () => {
                recargarTabla();
            });
        } catch (error) {
            // Error ya manejado
        }
    }
});
