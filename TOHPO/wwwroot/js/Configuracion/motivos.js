$(document).ready(function () {
    var tabla = $('#tablaMotivos').DataTable({
        ajax: {
            url: '/Configuracion/Motivo_Recordatorio/Index?handler=Motivos',
            dataSrc: ''
        },

        columns: [
            { data: 'id' },
            { data: 'descripcion' },
            {
                data: 'id',
                render: function (data) {
                    return `
                        <div class="d-flex justify-content-center gap-2">
                            <button class="btn btn-warning btn-sm btnEditar" data-id="${data}">Editar</button>
                            <button class="btn btn-danger btn-sm btnEliminar" data-id="${data}">Eliminar</button>
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
                targets: [1],
                className: "text-start"
            },
            {
                targets: [0, 2],
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

    // Eliminar motivo
    $('#tablaMotivos').on('click', '.btnEliminar', function () {
        const id = $(this).data('id');

        if (confirm('¿Desea eliminar este motivo de recordatorio?')) {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            fetch('/Configuracion/Motivo_Recordatorio/Index?handler=Eliminar', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    ...(token && { 'RequestVerificationToken': token })
                },
                body: 'id=' + encodeURIComponent(id)
            })
                .then(async response => {
                    const text = await response.text();
                    try {
                        return JSON.parse(text);
                    } catch {
                        console.error("Respuesta no es JSON:", text);
                        throw new Error("El servidor no devolvió JSON");
                    }
                })
                .then(data => {
                    if (data.success) {
                        tabla.ajax.reload(null, false);
                    } else {
                        alert('Error: ' + (data.message || data.error || 'No se pudo eliminar'));
                    }
                })
                .catch(err => console.error("Error en fetch:", err));
        }
    });

    // Editar motivo
    $('#tablaMotivos').on('click', '.btnEditar', function () {
        const id = $(this).data('id');
        window.location.href = `/Configuracion/Motivo_Recordatorio/Upsert/${id}`;
    });
});