$(document).ready(function () {
    var tabla = $('#tablaClientes').DataTable({
        ajax: {
            url: '/Configuracion/Clientes/Index?handler=Clientes',
            dataSrc: ''
        },

        columns: [
            { data: 'id' },
            { data: 'cedula' },
            {
                data: null,
                render: function (data, type, row) {
                    return row.nombre + ' ' + row.primer_Apellido + ' ' + row.segundo_Apellido;
                }
            },
            {
                data: 'id',
                render: function (data) {
                    return `
                        <div class="d-flex justify-content-center gap-2">
                            <button class="btn btn-success btn-sm btnDetalles" data-id="${data}">Detalles</button>
                            <button class="btn btn-warning btn-sm btnEditar" data-id="${data}">Editar</button>
                            <button class="btn btn-danger btn-sm btnEliminar" data-id="${data}">Eliminar</button>
                        </div>
                    `;
                }
            }
        ],
        columnDefs: [
            {
                targets: "_all",      // todas las columnas
                className: "align-middle"  // centra verticalmente
            },
            {
                targets: [1, 2],   // columnas: id, cedula, nombre
                className: "text-start" // alineado a la izquierda
            },
            {
                targets: [0, 3],         // columna de acciones
                className: "text-center" // centrado horizontal
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



    $('#tablaClientes').on('click', '.btnEliminar', function () {
        const id = $(this).data('id');

        if (confirm('¿Desea eliminar este cliente?')) {
            // Obtiene el token antiforgery desde el input hidden del formulario Razor
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            fetch('/Configuracion/Clientes/Index?handler=Eliminar', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    ...(token && { 'RequestVerificationToken': token }) // agrega el token si existe
                },
                body: 'id=' + encodeURIComponent(id)
            })
                .then(async response => {
                    const text = await response.text(); // obtenemos la respuesta como texto
                    try {
                        return JSON.parse(text); // intentamos parsear como JSON
                    } catch {
                        console.error("Respuesta no es JSON:", text);
                        throw new Error("El servidor no devolvió JSON");
                    }
                })
                .then(data => {
                    if (data.success) {
                        tabla.ajax.reload(null, false); // recarga tabla, mantiene paginación
                    } else {
                        alert('Error: ' + (data.error || 'No se pudo eliminar'));
                    }
                })
                .catch(err => console.error("Error en fetch:", err));
        }
    });




    $('#tablaClientes').on('click', '.btnDetalles', function () {
        const fila = tabla.row($(this).parents('tr')).data();

        $('#modalId').text(fila.id);
        $('#modalCedula').text(fila.cedula);
        $('#modalNombre').text(fila.nombre);
        $('#modalPrimerApellido').text(fila.primer_Apellido);
        $('#modalSegundoApellido').text(fila.segundo_Apellido);
        $('#modalCorreo').text(fila.correo_Electronico || 'No registrado');
        $('#modalTelefono').text(fila.telefono || 'No registrado');

        var modal = new bootstrap.Modal(document.getElementById('detallesModal'));
        modal.show();
    });

    // Nuevo: acceso al Upsert para editar cliente
    $('#tablaClientes').on('click', '.btnEditar', function () {
        const id = $(this).data('id');
        window.location.href = `/Configuracion/Clientes/Upsert/${id}`;
    });
});
