$(document).ready(function () {
    var tabla = $('#tablaMetodosPago').DataTable({
        ajax: {
            url: '/Configuracion/Metodos_Pago/Index?handler=MetodosPago',
            dataSrc: ''
        },
        columns: [
            { data: 'id' },
            { data: 'descripcion' },
            {
                data: 'id',
                render: function (data) {
                    return `
                        <button class="btn btn-warning btn-sm btnEditar" data-id="${data}">Editar</button>
                        <button class="btn btn-danger btn-sm btnEliminar" data-id="${data}">Eliminar</button>
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
                targets: [1],   // columnas: id, cedula, nombre
                className: "text-start" // alineado a la izquierda
            },
            {
                targets: [0, 2],         // columna de acciones
                className: "text-center" // centrado horizontal
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

    $('#tablaMetodosPago').on('click', '.btnEliminar', function () {
        const id = $(this).data('id');
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (confirm('¿Desea eliminar este método de pago?')) {
            fetch('/Configuracion/Metodos_Pago/Index?handler=Eliminar', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    ...(token && { 'RequestVerificationToken': token })
                },
                body: 'id=' + encodeURIComponent(id)
            })
                .then(async response => {
                    const text = await response.text();
                    try { return JSON.parse(text); } catch { throw new Error("El servidor no devolvió JSON"); }
                })
                .then(data => {
                    if (data.success) tabla.ajax.reload(null, false);
                    else alert('Error: ' + (data.error || 'No se pudo eliminar'));
                })
                .catch(err => console.error("Error en fetch:", err));
        }
    });

    $('#tablaMetodosPago').on('click', '.btnEditar', function () {
        const id = $(this).data('id');
        window.location.href = `/Configuracion/Metodos_Pago/Upsert/${id}`;
    });
});