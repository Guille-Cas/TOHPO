let calendar;
let tablaRecordatoriosDataTable;

$(document).ready(function() {
    // Inicializar DataTable
    inicializarDataTable();
    
    // Cargar recordatorios
    cargarRecordatorios();
    
    // Configurar eventos
    configurarEventos();
    
    // Inicializar calendario
    inicializarCalendario();
});

function inicializarDataTable() {
    tablaRecordatoriosDataTable = $('#tablaRecordatorios').DataTable({
        language: {
            url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json'
        },
        pageLength: 10,
        searching: true,
        responsive: true,
        order: [[0, 'asc']], // Ordenar por fecha
        columnDefs: [
            {
                targets: [6], // Columna de acciones
                orderable: false,
                searchable: false
            },
            {
                targets: [4], // Columna de recurrencia
                width: '150px'
            }
        ]
    });
}

function cargarRecordatorios() {
    $.get('/Recordatorios/Index?handler=Recordatorios')
        .done(function(data) {
            if (data.error) {
                console.error('Error al cargar recordatorios:', data.error);
                mostrarMensaje('Error al cargar los recordatorios', 'error');
                return;
            }
            
            // Limpiar tabla
            tablaRecordatoriosDataTable.clear();
            
            // Agregar datos
            data.forEach(function(recordatorio) {
                const estado = obtenerEstado(recordatorio.fechaCompleta);
                const badgeEstado = obtenerBadgeEstado(estado);
                const badgeRecurrencia = obtenerBadgeRecurrencia(recordatorio);
                
                const fila = [
                    recordatorio.fecha_Hora,
                    recordatorio.cliente,
                    recordatorio.motivo,
                    recordatorio.detalles || '<span class="text-muted">Sin detalles</span>',
                    badgeRecurrencia,
                    badgeEstado,
                    generarBotonesAccion(recordatorio)
                ];
                
                tablaRecordatoriosDataTable.row.add(fila);
            });
            
            // Refrescar tabla
            tablaRecordatoriosDataTable.draw();
            
            // Actualizar calendario si está visible
            if (calendar) {
                cargarEventosCalendario();
            }
        })
        .fail(function() {
            mostrarMensaje('Error de conexión al cargar recordatorios', 'error');
        });
}

function obtenerEstado(fechaCompleta) {
    const ahora = new Date();
    const fecha = new Date(fechaCompleta);
    const diferenciaDias = (fecha - ahora) / (1000 * 60 * 60 * 24);
    
    if (diferenciaDias < 0) {
        return 'Vencido';
    } else if (diferenciaDias <= 1) {
        return 'Próximo';
    } else {
        return 'Programado';
    }
}

function obtenerBadgeEstado(estado) {
    const badges = {
        'Vencido': '<span class="badge bg-danger">Vencido</span>',
        'Próximo': '<span class="badge bg-warning text-dark">Próximo</span>',
        'Programado': '<span class="badge bg-success">Programado</span>'
    };
    
    return badges[estado] || '<span class="badge bg-secondary">Desconocido</span>';
}

function obtenerBadgeRecurrencia(recordatorio) {
    if (recordatorio.esRecurrente) {
        return '<span class="badge bg-info">' + recordatorio.recurrencia + '</span>';
    } else if (recordatorio.recordatorioPadreId) {
        return '<span class="badge bg-secondary">Serie</span>';
    } else {
        return '<span class="badge bg-light text-dark">Único</span>';
    }
}

function generarBotonesAccion(recordatorio) {
    return `
        <div class="btn-group btn-group-sm" role="group">
            <button type="button" class="btn btn-info btnDetalles me-2 rounded" 
                    data-id="${recordatorio.id}" 
                    title="Ver detalles">
                Detalles
            </button>
            <a href="/Recordatorios/Upsert/${recordatorio.id}" 
               class="btn btn-warning me-2 rounded" 
               title="Editar">
                Editar
            </a>
            <button type="button" class="btn btn-danger btnEliminar me-2 rounded" 
                    data-id="${recordatorio.id}" 
                    title="Eliminar">
                Eliminar
            </button>
        </div>
    `;
}

function configurarEventos() {
    // Filtros de estado
    $('[data-filtro]').on('click', function() {
        const filtro = $(this).data('filtro');
        
        // Actualizar botones activos
        $('[data-filtro]').removeClass('active');
        $(this).addClass('active');
        
        // Aplicar filtro
        aplicarFiltro(filtro);
    });
    
    // Ver detalles
    $(document).on('click', '.btnDetalles', function() {
        const id = $(this).data('id');
        mostrarDetalles(id);
    });
    
    // Eliminar recordatorio
    $(document).on('click', '.btnEliminar', function() {
        const id = $(this).data('id');
        eliminarRecordatorio(id);
    });
    
    // Alternar vista
    $('#toggleView').on('click', function() {
        alternarVista();
    });
}

function aplicarFiltro(filtro) {
    if (filtro === 'todos') {
        tablaRecordatoriosDataTable.search('').draw();
    } else if (filtro === 'recurrente') {
        tablaRecordatoriosDataTable.search('Serie|Diario|Semanal|Mensual|Anual', true, false).draw();
    } else {
        tablaRecordatoriosDataTable.search(filtro).draw();
    }
}

function mostrarDetalles(id) {
    $.get(`/Recordatorios/Index?handler=Detalles&id=${id}`)
        .done(function(data) {
            if (data.error) {
                mostrarMensaje('Error al cargar los detalles', 'error');
                return;
            }
            
            let contenido = `
                <div class="row g-3">
                    <div class="col-12">
                        <div class="card border-0 bg-light">
                            <div class="card-body">
                                <h6 class="text-primary mb-3">
                                    Información del Recordatorio
                                </h6>
                                <div class="row g-2">
                                    <div class="col-md-6">
                                        <strong>📅 Fecha y Hora:</strong><br>
                                        <span class="text-primary">${data.fechaHora}</span>
                                    </div>
                                    <div class="col-md-6">
                                        <strong>📊 Estado:</strong><br>
                                        <span class="badge bg-${getEstadoClass(data.estado)}">${data.estado}</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    <div class="col-md-6">
                        <div class="card border-0 bg-light">
                            <div class="card-body">
                                <h6 class="text-info mb-3">
                                    Cliente
                                </h6>
                                <p class="mb-2"><strong>Nombre:</strong> ${data.cliente}</p>
                                ${data.clienteTelefono ? `<p class="mb-2"><strong>Teléfono:</strong> ${data.clienteTelefono}</p>` : ''}
                                ${data.clienteEmail ? `<p class="mb-0"><strong>Email:</strong> ${data.clienteEmail}</p>` : ''}
                            </div>
                        </div>
                    </div>
                    
                    <div class="col-md-6">
                        <div class="card border-0 bg-light">
                            <div class="card-body">
                                <h6 class="text-warning mb-3">
                                    Motivo
                                </h6>
                                <p class="mb-0">${data.motivo}</p>
                            </div>
                        </div>
                    </div>
                    
                    ${data.detalles ? `
                    <div class="col-12">
                        <div class="card border-0 bg-light">
                            <div class="card-body">
                                <h6 class="text-secondary mb-3">
                                    Detalles
                                </h6>
                                <p class="mb-0">${data.detalles}</p>
                            </div>
                        </div>
                    </div>` : ''}
            `;
            
            // Información de recurrencia
            if (data.esRecurrente || data.esParteDeSerie) {
                contenido += `
                    <div class="col-12">
                        <div class="card border-0 bg-success bg-opacity-10">
                            <div class="card-body">
                                <h6 class="text-success mb-3">
                                    Información de Recurrencia
                                </h6>
                `;
                
                if (data.esRecurrente) {
                    contenido += `
                        <p class="mb-2"><strong>Tipo:</strong> ${data.tipoRecurrencia}</p>
                        <p class="mb-2"><strong>Intervalo:</strong> Cada ${data.intervaloRecurrencia} ${getTipoTexto(data.tipoRecurrencia)}</p>
                        ${data.fechaFinRecurrencia ? `<p class="mb-2"><strong>Termina:</strong> ${data.fechaFinRecurrencia}</p>` : ''}
                        ${data.maximoRepeticiones ? `<p class="mb-2"><strong>Máximo repeticiones:</strong> ${data.maximoRepeticiones}</p>` : ''}
                        <p class="mb-0"><strong>Recordatorios generados:</strong> ${data.numeroDeHijos}</p>
                    `;
                } else {
                    contenido += `<p class="mb-0">Este recordatorio forma parte de una serie recurrente.</p>`;
                }
                
                contenido += `
                            </div>
                        </div>
                    </div>
                `;
            }
            
            contenido += '</div>';
            
            $('#contenidoModalDetalles').html(contenido);
            $('#modalDetalles').modal('show');
        })
        .fail(function() {
            mostrarMensaje('Error al cargar los detalles', 'error');
        });
}

function getEstadoClass(estado) {
    const classes = {
        'Vencido': 'danger',
        'Próximo': 'warning',
        'Programado': 'success'
    };
    return classes[estado] || 'secondary';
}

function getTipoTexto(tipo) {
    const textos = {
        'Diario': 'día(s)',
        'Semanal': 'semana(s)',
        'Mensual': 'mes(es)',
        'Anual': 'año(s)'
    };
    return textos[tipo] || 'período(s)';
}

function eliminarRecordatorio(id) {
    if (!confirm('¿Está seguro de que desea eliminar este recordatorio?\n\nSi es un recordatorio recurrente, se eliminarán todos los recordatorios de la serie.')) {
        return;
    }
    
    const token = $('input[name="__RequestVerificationToken"]').val();
    
    $.post('/Recordatorios/Index?handler=Eliminar', {
        id: id,
        __RequestVerificationToken: token
    })
    .done(function(response) {
        if (response.success) {
            mostrarMensaje('Recordatorio eliminado correctamente', 'success');
            cargarRecordatorios();
        } else {
            mostrarMensaje(response.message || 'Error al eliminar el recordatorio', 'error');
        }
    })
    .fail(function() {
        mostrarMensaje('Error de conexión al eliminar el recordatorio', 'error');
    });
}

function alternarVista() {
    const $vistaLista = $('#vistaLista');
    const $vistaCalendario = $('#vistaCalendario');
    const $btnToggle = $('#toggleView');
    
    if ($vistaLista.is(':visible')) {
        // Cambiar a vista calendario
        $vistaLista.hide();
        $vistaCalendario.show();
        $btnToggle.html('Vista Lista');
        
        if (!calendar) {
            inicializarCalendario();
        } else {
            cargarEventosCalendario();
        }
    } else {
        // Cambiar a vista lista
        $vistaCalendario.hide();
        $vistaLista.show();
        $btnToggle.html('Vista Calendario');
    }
}

function inicializarCalendario() {
    const calendarEl = document.getElementById('calendario');
    if (!calendarEl) return;
    
    calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'dayGridMonth',
        locale: 'es',
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay'
        },
        buttonText: {
            today: 'Hoy',
            month: 'Mes',
            week: 'Semana',
            day: 'Día'
        },
        height: 600,
        eventClick: function(info) {
            const id = parseInt(info.event.id);
            mostrarDetalles(id);
        },
        eventDidMount: function(info) {
            // Agregar tooltip
            info.el.setAttribute('title', info.event.extendedProps.description || '');
        }
    });
    
    calendar.render();
    cargarEventosCalendario();
}

function cargarEventosCalendario() {
    if (!calendar) return;
    
    $.get('/Recordatorios/Index?handler=CalendarEvents')
        .done(function(data) {
            if (data.error) {
                console.error('Error al cargar eventos:', data.error);
                return;
            }
            
            // Limpiar eventos existentes
            calendar.removeAllEvents();
            
            // Agregar nuevos eventos
            calendar.addEventSource(data);
        })
        .fail(function() {
            mostrarMensaje('Error al cargar eventos del calendario', 'error');
        });
}

function mostrarMensaje(mensaje, tipo) {
    // Implementar sistema de notificaciones (puede usar Toastr, SweetAlert, etc.)
    if (tipo === 'success') {
        alert('✅ ' + mensaje);
    } else {
        alert('❌ ' + mensaje);
    }
}