// JavaScript para la página de Upsert de Compras
// Maneja auto-focus, búsqueda de productos y actualización de hora en tiempo real

document.addEventListener('DOMContentLoaded', function() {
    // 1. Auto-focus en el buscador de productos al cargar la página
    const buscadorProducto = document.getElementById('buscarEnModalCompra');
    const modalProductos = document.getElementById('modalSeleccionarProducto');
    
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
                filtrarProductosCompra();
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
                    buscarProductoPorCodigoEnModalCompra(inputValue);
                }, 100); // Pequeño delay para códigos de barras
            }
        });
    }

    // 3. Actualizar hora automáticamente cada minuto
    actualizarHoraEnTiempoRealCompra();
    setInterval(actualizarHoraEnTiempoRealCompra, 60000); // Actualizar cada minuto

    // 4. Atajos de teclado para navegación rápida
    document.addEventListener('keydown', function(e) {
        // F2 para abrir selector de productos
        if (e.key === 'F2') {
            e.preventDefault();
            const btnSeleccionarProducto = document.getElementById('btnSeleccionarProducto');
            if (btnSeleccionarProducto) {
                btnSeleccionarProducto.click();
            }
        }
        
        // F3 para agregar producto
        if (e.key === 'F3') {
            e.preventDefault();
            const btnAgregarProductoCompra = document.getElementById('btnAgregarProducto');
            if (btnAgregarProductoCompra) {
                btnAgregarProductoCompra.click();
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
    configurarNavegacionOptimizadaCompra();

    // 6. Configurar validación flexible de pagos
    configurarValidacionPagosFlexibleCompra();
});

// Función para actualizar la hora en tiempo real
function actualizarHoraEnTiempoRealCompra() {
    const campoHora = document.querySelector('input[name="Compra.Hora"]');
    if (campoHora) {
        const ahora = new Date();
        const horaFormateada = ahora.getHours().toString().padStart(2, '0') + ':' + 
                              ahora.getMinutes().toString().padStart(2, '0');
        campoHora.value = horaFormateada;
    }
}

// Función para buscar producto por código en el modal
function buscarProductoPorCodigoEnModalCompra(codigo) {
    // Buscar en la tabla del modal si el producto con ese código está visible
    const filas = document.querySelectorAll('#tablaProductosModalCompra tr');
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
function configurarNavegacionOptimizadaCompra() {
    const campos = [
        'select[name="Compra.Id_Proveedor"]',
        '#inputCantidad',
        '#inputCostoUnitario',
        '#inputDescuento',
        '#selectMetodoPagoCompra',
        '#inputMontoMetodoCompra'
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

// Función para configurar validación flexible de métodos de pago en compras
function configurarValidacionPagosFlexibleCompra() {
    // Sobrescribir la función de validación existente
    window.actualizarValidacionPagos = function() {
        const totalCompra = parseFloat(document.getElementById('totalDisplay').textContent.replace('₡', '').replace(',', '')) || 0;
        const totalPagos = window.metodosPagoData ? window.metodosPagoData.reduce((sum, metodo) => sum + metodo.monto, 0) : 0;
        
        const alerta = document.getElementById('alertaPagos');
        
        if (totalPagos < totalCompra) {
            // Pago insuficiente - mostrar error
            alerta.style.display = 'block';
            alerta.className = 'alert alert-danger';
            alerta.innerHTML = `
                <strong>¡Atención!</strong> 
                Total Compra: ₡${totalCompra.toFixed(2)} - 
                Total Pagos: ₡${totalPagos.toFixed(2)} - 
                <strong>Faltante: ₡${(totalCompra - totalPagos).toFixed(2)}</strong>
            `;
        } else if (totalPagos > totalCompra) {
            // Pago superior - mostrar información de pago adelantado
            alerta.style.display = 'block';
            alerta.className = 'alert alert-success';
            alerta.innerHTML = `
                <strong>Pago Realizado:</strong> ₡${totalPagos.toFixed(2)} | 
                <strong>Total Compra:</strong> ₡${totalCompra.toFixed(2)} | 
                <strong>Pago adelantado/exceso:</strong> ₡${(totalPagos - totalCompra).toFixed(2)}
            `;
        } else {
            // Pago exacto - ocultar alerta
            alerta.style.display = 'none';
        }
    };
}

// Función para mostrar alertas con SweetAlert en lugar de alert()
function mostrarAlertaProductoCompra(mensaje, tipo = 'warning') {
    Swal.fire({
        icon: tipo,
        title: tipo === 'error' ? 'Error' : tipo === 'success' ? 'Éxito' : 'Atención',
        text: mensaje,
        confirmButtonColor: tipo === 'error' ? '#d33' : '#28a745'
    });
}

// Función para seleccionar producto en compras con SweetAlert
function seleccionarProductoCompra(codigo, nombre, costo, existencia, impuesto) {
    // Función para selección de producto en compras
    document.getElementById('inputBuscarProducto').value = `${codigo} - ${nombre}`;
    document.getElementById('inputCostoUnitario').value = costo || 0;
    
    // Almacenar datos del producto
    const inputBuscar = document.getElementById('inputBuscarProducto');
    inputBuscar.setAttribute('data-codigo', codigo);
    inputBuscar.setAttribute('data-impuesto', impuesto);
    
    // Enfocar automáticamente el campo de cantidad tras la selección
    setTimeout(() => {
        const cantidadInput = document.getElementById('inputCantidad');
        if (cantidadInput) {
            cantidadInput.focus();
            cantidadInput.select();
        }
    }, 100);
    
    // Cerrar modal
    const modalInstance = bootstrap.Modal.getInstance(document.getElementById('modalSeleccionarProducto'));
    if (modalInstance) {
        modalInstance.hide();
    }
    
    // Toast de confirmación
    Swal.fire({
        toast: true,
        position: 'top-end',
        icon: 'success',
        title: `Producto ${nombre} seleccionado para compra`,
        showConfirmButton: false,
        timer: 2000
    });
}

// Función para mostrar información sobre las mejoras implementadas
function mostrarInfoMejorasCompra() {
    const mensaje = `
    🚀 Mejoras implementadas en el proceso de compra:
    
    ✅ Auto-focus en buscador de productos al abrir el modal
    ✅ Búsqueda automática por código de barras (8+ caracteres)
    ✅ Actualización automática de hora cada minuto
    ✅ Atajos de teclado:
        • F2: Abrir selector de productos
        • F3: Agregar producto rápidamente  
        • Enter: Buscar en el modal
        • Escape: Cerrar modal
    ✅ Navegación optimizada con Tab entre campos
    ✅ Validación flexible de métodos de pago (permite montos superiores)
    ✅ SweetAlert para mejor experiencia de usuario
    
    ¡Flujo de trabajo optimizado para mayor productividad!
    `;
    
    console.log(mensaje);
}

// Mostrar información de mejoras al cargar
setTimeout(mostrarInfoMejorasCompra, 1000);}