$(document).ready(function () {
    var tabla = $('#tablaProveedores').DataTable({
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
                data: 'id',
                render: function (data) {
                    return `
                        <button class="btn btn-warning btn-sm btnEditar" data-id="${data}">Editar</button>
                        <button class="btn btn-danger btn-sm btnEliminar" data-id="${data}">Eliminar</button>
                    `;
                }
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

    $('#tablaProveedores').on('click', '.btnEliminar', function () {
        const id = $(this).data('id');
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (confirm('¿Desea eliminar este proveedor?')) {
            fetch('/Configuracion/Proveedores/Index?handler=Eliminar', {
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

    $('#tablaProveedores').on('click', '.btnEditar', function () {
        const id = $(this).data('id');
        window.location.href = `/Configuracion/Proveedores/Upsert/${id}`;
    });
});