$(document).ready(function () {
    var tabla = $('#tablaImpuestos').DataTable({
        ajax: {
            url: '/Configuracion/Impuestos/Index?handler=Impuestos',
            dataSrc: ''
        },
        columns: [
            { data: 'id' },
            { data: 'descripcion' },
            {
                data: 'porcentaje',
                render: function (data) {
                    return data + ' %';
                }
            },
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
                targets: [1, 2],   // columnas: id, cedula, nombre
                className: "text-start" // alineado a la izquierda
            },
            {
                targets: [0, 3],         // columna de acciones
                className: "text-center" // centrado horizontal
            }
        ],
        language: {
            emptyTable: "No hay impuestos registrados",
            loadingRecords: "Cargando...",
            search: "Buscar:",
            lengthMenu: "Mostrar _MENU_ registros",
            info: "Mostrando _START_ a _END_ de _TOTAL_ impuestos",
            infoEmpty: "Mostrando 0 impuestos",
            paginate: {
                first: "Primero",
                last: "Último",
                next: "Siguiente",
                previous: "Anterior"
            }
        }
    });

    $('#tablaImpuestos').on('click', '.btnEliminar', function () {
        const id = $(this).data('id');
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (confirm('¿Desea eliminar este impuesto?')) {
            fetch('/Configuracion/Impuestos/Index?handler=Eliminar', {
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

    $('#tablaImpuestos').on('click', '.btnEditar', function () {
        const id = $(this).data('id');
        window.location.href = `/Configuracion/Impuestos/Upsert/${id}`;
    });
});