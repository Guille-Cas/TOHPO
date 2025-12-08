$(document).ready(function () {
    let tabla;

    // Inicializar DataTable
    function inicializarTabla() {
        tabla = $('#tablaMotivos').DataTable({
            ajax: {
                url: '/Configuracion/Motivo_Recordatorio/Index?handler=Motivos',
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
                        const detallesBtn = `<button class="btn btn-info btn-sm btnDetalles" data-id="${data}">Detalles</button>`;
                        const editarBtn = `<button class="btn btn-warning btn-sm btnEditar" data-id="${data}">Editar</button>`;
                        const eliminarBtn = `<button class="btn btn-danger btn-sm btnEliminar" data-id="${data}">Eliminar</button>`;
                        
                        return `
                            <div class="d-flex justify-content-center gap-2">
                                ${detallesBtn}
                                ${editarBtn}
                                ${eliminarBtn}
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
                emptyTable: "No hay motivos registrados en el sistema",
                loadingRecords: "Cargando...",
                search: "Buscar:",
                lengthMenu: "Mostrar _MENU_ registros",
                info: "Mostrando _START_ a _END_ de _TOTAL_ motivos",
                infoEmpty: "Mostrando 0 motivos",
                paginate: {
                    first: "Primero",
                    last: "Último",
                    next: "Siguiente",
                    previous: "Anterior"
                }
            }
        });
    }

    // Función para mostrar feedback
    function mostrarFeedback(tipo, mensaje, titulo = null) {
        // Usar SweetAlert2 si está disponible, sino usar alert
        if (typeof Swal !== 'undefined') {
            let icon = tipo;
            if (tipo === 'validation') icon = 'warning';
            
            Swal.fire({
                icon: icon,
                title: titulo || (tipo === 'success' ? 'Éxito' : tipo === 'error' ? 'Error' : 'Información'),
                text: mensaje,
                timer: tipo === 'success' ? 3000 : null,
                showConfirmButton: tipo !== 'success'
            });
        } else {
            // Fallback a alert
            alert(`${titulo || tipo.toUpperCase()}: ${mensaje}`);
        }
    }

    // Función para confirmar acción
    function confirmarAccion(mensaje, callback) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: '¿Está seguro?',
                text: mensaje,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'Sí, continuar',
                cancelButtonText: 'Cancelar'
            }).then((result) => {
                if (result.isConfirmed) {
                    callback();
                }
            });
        } else {
            if (confirm(mensaje)) {
                callback();
            }
        }
    }

    // Función para manejar respuestas del servidor
    function manejarRespuesta(data, callbackExito = null) {
        if (data.success) {
            mostrarFeedback('success', data.message);
            if (callbackExito) callbackExito();
        } else {
            switch (data.type) {
                case 'validation':
                    if (data.shouldDeactivate) {
                        mostrarDialogoDesactivacion(data);
                    } else {
                        mostrarFeedback('warning', data.message);
                    }
                    break;
                case 'error':
                default:
                    mostrarFeedback('error', data.message);
                    break;
            }
        }
    }

    // Función para mostrar diálogo de desactivación
    function mostrarDialogoDesactivacion(data) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: 'No se puede eliminar',
                text: data.message,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#ffc107',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Sí, desactivar',
                cancelButtonText: 'Cancelar'
            }).then((result) => {
                if (result.isConfirmed) {
                    // Extraer el ID del contexto actual
                    const id = $('#tablaMotivos .btnEliminar:focus').data('id') || 
                              $('#tablaMotivos .btnEliminar').last().data('id');
                    if (id) {
                        desactivarMotivo(id);
                    }
                }
            });
        } else {
            const confirmar = confirm(`${data.message}\n\n¿Desea desactivar el motivo en su lugar?`);
            if (confirmar) {
                const id = $('#tablaMotivos .btnEliminar:focus').data('id') || 
                          $('#tablaMotivos .btnEliminar').last().data('id');
                if (id) {
                    desactivarMotivo(id);
                }
            }
        }
    }

    // Función para obtener token CSRF
    function obtenerToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    }

    // Función para hacer peticiones AJAX
    async function hacerPeticion(url, datos) {
        try {
            const token = obtenerToken();
            if (token) {
                datos += `&__RequestVerificationToken=${encodeURIComponent(token)}`;
            }

            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: datos
            });

            const text = await response.text();
            try {
                return JSON.parse(text);
            } catch {
                console.error("Respuesta no es JSON:", text);
                throw new Error("El servidor no devolvió JSON");
            }
        } catch (error) {
            console.error("Error en petición:", error);
            mostrarFeedback('error', 'Error de conexión con el servidor');
            throw error;
        }
    }

    // Inicializar tabla
    inicializarTabla();

    // Editar motivo - redirigir a la página Upsert
    $('#tablaMotivos').on('click', '.btnEditar', function () {
        const id = $(this).data('id');
        window.location.href = `/Configuracion/Motivo_Recordatorio/Upsert?id=${id}`;
    });

    // Eliminar motivo
    $('#tablaMotivos').on('click', '.btnEliminar', function () {
        const id = $(this).data('id');
        const fila = tabla.row($(this).parents('tr')).data();
        
        confirmarAccion(`¿Desea eliminar el motivo "${fila.descripcion}"?`, () => {
            eliminarMotivo(id);
        });
    });

    async function eliminarMotivo(id) {
        try {
            const datos = `id=${id}`;
            const resultado = await hacerPeticion('/Configuracion/Motivo_Recordatorio/Index?handler=Eliminar', datos);
            
            manejarRespuesta(resultado, () => {
                tabla.ajax.reload(null, false);
            });
        } catch (error) {
            // Error ya manejado en hacerPeticion
        }
    }

    async function desactivarMotivo(id) {
        try {
            const datos = `id=${id}`;
            const resultado = await hacerPeticion('/Configuracion/Motivo_Recordatorio/Index?handler=Desactivar', datos);
            
            manejarRespuesta(resultado, () => {
                tabla.ajax.reload(null, false);
            });
        } catch (error) {
            // Error ya manejado en hacerPeticion
        }
    }

    // Mostrar detalles
    $('#tablaMotivos').on('click', '.btnDetalles', function () {
        const fila = tabla.row($(this).parents('tr')).data();

        $('#modalId').text(fila.id);
        $('#modalDescripcion').text(fila.descripcion);
        $('#modalEstado').text(fila.estado ? 'Activo' : 'Inactivo');

        var modal = new bootstrap.Modal(document.getElementById('detallesModal'));
        modal.show();
    });
});