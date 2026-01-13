$(document).ready(function () {
    var tabla = $('#tablaPresentaciones').DataTable({
        ajax: {
            url: '/Configuracion/Presentaciones/Index?handler=Presentaciones',
            dataSrc: ''
        },
        columns: [
            { data: 'id' },
            { data: 'cantidad' },
            { data: 'unidad_Medida' },
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
                targets: [2],   // columnas: id, cedula, nombre
                className: "text-start" // alineado a la izquierda
            },
            {
                targets: [0, 3],         // columna de acciones
                className: "text-center" // centrado horizontal
            },
            {
                targets: [1],   // columnas: id, cedula, nombre
                className: "text-end" // alineado a la izquierda
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

    $('#tablaPresentaciones').on('click', '.btnEliminar', function () {
        const id = $(this).data('id');
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (confirm('¿Desea eliminar esta presentación?')) {
            fetch('/Configuracion/Presentaciones/Index?handler=Eliminar', {
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

    $('#tablaPresentaciones').on('click', '.btnEditar', function () {
        const id = $(this).data('id');
        window.location.href = `/Configuracion/Presentaciones/Upsert/${id}`;
    });
});