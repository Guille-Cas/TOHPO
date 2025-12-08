// JavaScript para la página de Upsert de Ventas
// Maneja auto-focus, búsqueda de productos y actualización de hora en tiempo real

document.addEventListener('DOMContentLoaded', function() {
    // 1. Auto-focus en el buscador de productos al cargar la página
    const buscadorProducto = document.getElementById('buscarEnModal');
    const modalProductos = document.getElementById('modalSeleccionProductos');
    
    // Enfocar automáticamente el buscador cuando se abre el modal
    if (modalProductos) {
        modalProductos.addEventListener('shown.bs.modal', function() {
            if (buscadorProducto) {
                buscadorProducto.focus();
                buscadorProducto.select();
            }
        });
    }

    // 2. Búsqueda rápida con Enter en el modal
    if (buscadorProducto) {
        buscadorProducto.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                filtrarProductos();
            }
        });

        // Búsqueda automática por código de barras
        let codigoBuffer = '';
        let timeoutId = null;

        buscadorProducto.addEventListener('input', function(e) {
            const inputValue = e.target.value.trim();
            
            // Si el input tiene más de 8 caracteres, probablemente es un código de barras
            if (inputValue.length >= 8) {
                clearTimeout(timeoutId);
                timeoutId = setTimeout(() => {
                    buscarProductoPorCodigoEnModal(inputValue);
                }, 100); // Pequeño delay para códigos de barras
            }
        });
    }

    // 3. Actualizar hora automáticamente cada minuto
    actualizarHoraEnTiempoReal();
    setInterval(actualizarHoraEnTiempoReal, 60000); // Actualizar cada minuto

    // 4. Atajos de teclado para navegación rápida
    document.addEventListener('keydown', function(e) {
        // F2 para abrir selector de productos
        if (e.key === 'F2') {
            e.preventDefault();
            const btnBuscarProducto = document.getElementById('btnBuscarProducto');
            if (btnBuscarProducto) {
                btnBuscarProducto.click();
            }
        }
        
        // F3 para agregar producto
        if (e.key === 'F3') {
            e.preventDefault();
            const btnAgregarProducto = document.getElementById('btnAgregarProducto');
            if (btnAgregarProducto) {
                btnAgregarProducto.click();
            }
        }

        // Escape para cerrar modal y volver al flujo
        if (e.key === 'Escape' && modalProductos && modalProductos.classList.contains('show')) {
            const modalInstance = bootstrap.Modal.getInstance(modalProductos);
            if (modalInstance) {
                modalInstance.hide();
            }
        }
    });

    // 5. Mejorar navegación por Tab entre campos principales
    configurarNavegacionOptimizada();

    // 6. Configurar validación flexible de pagos
    configurarValidacionPagosFlexible();
});

// Función para actualizar la hora en tiempo real
function actualizarHoraEnTiempoReal() {
    const campoHora = document.querySelector('input[name="Venta.Hora"]');
    if (campoHora) {
        const ahora = new Date();
        const horaFormateada = ahora.getHours().toString().padStart(2, '0') + ':' + 
                              ahora.getMinutes().toString().padStart(2, '0');
        campoHora.value = horaFormateada;
    }
}

// Función para buscar producto por código en el modal
function buscarProductoPorCodigoEnModal(codigo) {
    // Buscar en la tabla del modal si el producto con ese código está visible
    const filas = document.querySelectorAll('#tablaProductosModal tr');
    let productoEncontrado = null;

    filas.forEach(fila => {
        const celdaCodigo = fila.querySelector('td:nth-child(2)');
        if (celdaCodigo && celdaCodigo.textContent.trim() === codigo) {
            productoEncontrado = fila;
        }
    });

    if (productoEncontrado) {
        // Resaltar la fila encontrada
        filas.forEach(f => f.classList.remove('table-warning'));
        productoEncontrado.classList.add('table-warning');
        
        // Hacer scroll hasta la fila
        productoEncontrado.scrollIntoView({ behavior: 'smooth', block: 'center' });
        
        // Auto-seleccionar si hay un botón de selección
        const btnSeleccionar = productoEncontrado.querySelector('button');
        if (btnSeleccionar) {
            // Dar un pequeño delay para que el usuario vea la selección
            setTimeout(() => {
                btnSeleccionar.click();
            }, 500);
        }
    }
}

// Configurar navegación optimizada con Tab
function configurarNavegacionOptimizada() {
    const campos = [
        'input[name="Venta.Id_Cliente"]',
        '#inputCantidad',
        '#inputPrecioUnitario',
        '#inputDescuento',
        '#selectMetodoPago',
        '#inputMontoMetodo'
    ];

    campos.forEach((selector, index) => {
        const campo = document.querySelector(selector);
        if (campo) {
            campo.addEventListener('keydown', function(e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    const siguienteCampo = document.querySelector(campos[index + 1]);
                    if (siguienteCampo) {
                        siguienteCampo.focus();
                        if (siguienteCampo.select) {
                            siguienteCampo.select();
                        }
                    }
                }
            });
        }
    });
}

// Función para configurar validación flexible de métodos de pago
function configurarValidacionPagosFlexible() {
    // Sobrescribir la función de validación existente
    window.actualizarValidacionPagos = function() {
        const totalVenta = parseFloat(document.getElementById('totalDisplay').textContent.replace('₡', '').replace(',', '')) || 0;
        const totalPagos = window.metodosPagoData ? window.metodosPagoData.reduce((sum, metodo) => sum + metodo.monto, 0) : 0;
        
        const alerta = document.getElementById('alertaPagos');
        
        if (totalPagos < totalVenta) {
            // Pago insuficiente - mostrar error
            alerta.style.display = 'block';
            alerta.className = 'alert alert-danger';
            alerta.innerHTML = `
                <strong>¡Atención!</strong> 
                Total Venta: ₡${totalVenta.toFixed(2)} - 
                Total Pagos: ₡${totalPagos.toFixed(2)} - 
                <strong>Faltante: ₡${(totalVenta - totalPagos).toFixed(2)}</strong>
            `;
        } else if (totalPagos > totalVenta) {
            // Pago superior - mostrar información de vuelto
            alerta.style.display = 'block';
            alerta.className = 'alert alert-info';
            alerta.innerHTML = `
                <strong>Pago Recibido:</strong> ₡${totalPagos.toFixed(2)} | 
                <strong>Total Venta:</strong> ₡${totalVenta.toFixed(2)} | 
                <strong>Vuelto a entregar:</strong> ₡${(totalPagos - totalVenta).toFixed(2)}
            `;
        } else {
            // Pago exacto - ocultar alerta
            alerta.style.display = 'none';
        }
    };
}

// Función para mostrar alertas con SweetAlert en lugar de alert()
function mostrarAlertaProducto(mensaje, tipo = 'warning') {
    Swal.fire({
        icon: tipo,
        title: tipo === 'error' ? 'Error' : tipo === 'success' ? 'Éxito' : 'Atención',
        text: mensaje,
        confirmButtonColor: tipo === 'error' ? '#d33' : '#28a745'
    });
}

// Sobrescribir la función global de seleccionarProducto para mejorar UX
function seleccionarProducto(codigo, nombre, precio, porcentajeImpuesto) {
    // Llenar campos
    $('#inputBuscarProducto').val(`${codigo} - ${nombre}`).data('codigo', codigo);
    $('#inputPrecioUnitario').val(precio);
    
    // Almacenar porcentaje de impuesto
    $('#inputBuscarProducto').data('porcentajeImpuesto', porcentajeImpuesto);
    
    // Cerrar modal
    $('#modalSeleccionProductos').modal('hide');
    
    // Enfocar cantidad con SweetAlert toast de confirmación
    $('#inputCantidad').focus().select();
    
    // Toast de confirmación
    Swal.fire({
        toast: true,
        position: 'top-end',
        icon: 'success',
        title: `Producto ${nombre} seleccionado`,
        showConfirmButton: false,
        timer: 2000
    });
}