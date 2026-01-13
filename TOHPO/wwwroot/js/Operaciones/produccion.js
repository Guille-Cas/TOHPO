function verDetalleReceta(id) {
    fetch(`?handler=DetalleReceta&id=${id}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                const receta = data.receta;
                document.getElementById('contenidoDetalleReceta').innerHTML = `
                    <div class="row">
                        <div class="col-md-6">
                            <p><strong>Descripción:</strong> ${receta.descripcion}</p>
                            <p><strong>Producto:</strong> ${receta.producto}</p>
                            <p><strong>Rendimiento:</strong> ${receta.rendimiento}</p>
                            <p><strong>Cantidad Empaque:</strong> ${receta.cantidadEmpaque}</p>
                            <p><strong>Fecha Creación:</strong> ${receta.fechaCreacion}</p>
                        </div>
                        <div class="col-md-6">
                            <p><strong>Detalle:</strong></p>
                            <p class="text-muted">${receta.detalle || 'Sin detalle'}</p>
                        </div>
                        <div class="col-12">
                            <p><strong>Instrucciones:</strong></p>
                            <div class="card card-body bg-light">
                                ${receta.instrucciones}
                            </div>
                        </div>
                    </div>
                `;
                new bootstrap.Modal(document.getElementById('detalleRecetaModal')).show();
            } else {
                alert('Error: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Error al cargar el detalle de la receta');
        });
}

function verDetalleProduccion(id) {
    fetch(`?handler=DetalleProduccion&id=${id}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                const produccion = data.produccion;
                let detallesHtml = '';
                
                if (produccion.detalles && produccion.detalles.length > 0) {
                    detallesHtml = produccion.detalles.map(detalle => `
                        <tr>
                            <td>${detalle.receta}</td>
                            <td>${detalle.producto}</td>
                            <td>${detalle.cantidadProgramada}</td>
                            <td>${detalle.cantidadProducida}</td>
                            <td>${detalle.fechaInicio || 'No iniciado'}</td>
                            <td>${detalle.fechaFin || 'En progreso'}</td>
                            <td><span class="badge ${ detalle.estado ? 'bg-warning' : 'bg-success'}">${detalle.estado ? 'En Progreso' : 'Terminado'}</span></td>
                        </tr>
                    `).join('');
                } else {
                    detallesHtml = '<tr><td colspan="7" class="text-center">No hay detalles de producción</td></tr>';
                }

                document.getElementById('contenidoDetalleProduccion').innerHTML = `
                    <div class="row mb-3">
                        <div class="col-md-6">
                            <p><strong>ID:</strong> ${produccion.id}</p>
                            <p><strong>Fecha:</strong> ${produccion.fecha}</p>
                            <p><strong>Obra:</strong> ${produccion.obra || 'Sin especificar'}</p>
                        </div>
                        <div class="col-md-6">
                            <p><strong>Descripción:</strong> ${produccion.descripcion}</p>
                            <p><strong>Fecha Planeada:</strong> ${produccion.fechaPlaneada}</p>
                            <p><strong>Estado:</strong> <span class="badge ${produccion.estado ? 'bg-success' : 'bg-secondary'}">${produccion.estado ? 'En Progreso' : 'Finalizada'}</span></p>
                        </div>
                    </div>
                    <div class="table-responsive">
                        <table class="table table-striped">
                            <thead class="table-dark">
                                <tr>
                                    <th>Receta</th>
                                    <th>Producto</th>
                                    <th>Cantidad Programada</th>
                                    <th>Cantidad Producida</th>
                                    <th>Fecha Inicio</th>
                                    <th>Fecha Fin</th>
                                    <th>Estado</th>
                                </tr>
                            </thead>
                            <tbody>
                                ${detallesHtml}
                            </tbody>
                        </table>
                    </div>
                `;
                new bootstrap.Modal(document.getElementById('detalleProduccionModal')).show();
            } else {
                alert('Error: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Error al cargar el detalle de la producción');
        });
}