// JavaScript para la página de Upsert de Ventas - MODO DIRECTO SIMPLIFICADO

document.addEventListener('DOMContentLoaded', function() {
    // Auto-focus en el campo principal de búsqueda
    const inputBuscarProducto = document.getElementById('inputBuscarProducto');
    if (inputBuscarProducto) {
        inputBuscarProducto.focus();
        
        // Configurar búsqueda automática en el campo principal con Enter
        inputBuscarProducto.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                buscarYAgregarProductoPorCodigo();
            }
        });

        // Usar la función separada
        inputBuscarProducto.addEventListener('input', limpiarDatosInput);
    }
    
    // Modal de productos - si existe
    const modalProductos = document.getElementById('modalSeleccionProductos');
    const buscadorProducto = document.getElementById('buscarEnModal');
    
    if (modalProductos && buscadorProducto) {
        // Enfocar automáticamente el buscador cuando se abre el modal
        modalProductos.addEventListener('shown.bs.modal', function() {
            buscadorProducto.focus();
            buscadorProducto.select();
        });

        // Búsqueda rápida con Enter en el modal
        buscadorProducto.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                if (typeof filtrarProductos === 'function') {
                    filtrarProductos();
                }
            }
        });

        // Búsqueda automática por código de barras en modal
        buscadorProducto.addEventListener('input', function(e) {
            const inputValue = e.target.value.trim();
            
            if (inputValue.length >= 8) {
                setTimeout(() => {
                    buscarProductoPorCodigoEnModal(inputValue);
                }, 100);
            }
        });
    }

    // Actualizar hora automáticamente cada minuto
    actualizarHoraEnTiempoReal();
    setInterval(actualizarHoraEnTiempoReal, 60000);

    // Atajos de teclado para navegación rápida
    document.addEventListener('keydown', function(e) {
        // F2 para abrir selector de productos
        if (e.key === 'F2') {
            e.preventDefault();
            const btnBuscarProducto = document.getElementById('btnBuscarProducto');
            if (btnBuscarProducto) {
                btnBuscarProducto.click();
            }
        }
        
        // F3 para agregar producto directamente
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

    // Inicializar funciones de apoyo
    configurarValidacionPagosFlexible();
    
    // Configurar botón agregar
    const btnAgregar = document.getElementById('btnAgregarProducto');
    if (btnAgregar) {
        btnAgregar.addEventListener('click', function(e) {
            e.preventDefault();
            agregarProductoManualmente();
        });
    }
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
            setTimeout(() => {
                btnSeleccionar.click();
            }, 500);
        }
    }
}

// Función para configurar validación flexible de métodos de pago
function configurarValidacionPagosFlexible() {
    if (typeof window.actualizarValidacionPagos === 'undefined') {
        window.actualizarValidacionPagos = function() {
            const totalVenta = parseFloat(document.getElementById('totalDisplay').textContent.replace('₡', '').replace(',', '')) || 0;
            const totalPagos = window.metodosPagoData ? window.metodosPagoData.reduce((sum, metodo) => sum + metodo.monto, 0) : 0;
            
            const alerta = document.getElementById('alertaPagos');
            if (!alerta) return;
            
            if (totalPagos < totalVenta) {
                alerta.style.display = 'block';
                alerta.className = 'alert alert-danger';
                alerta.innerHTML = `
                    <strong>¡Atención!</strong> 
                    Total Venta: ₡${totalVenta.toFixed(2)} - 
                    Total Pagos: ₡${totalPagos.toFixed(2)} - 
                    <strong>Faltante: ₡${(totalVenta - totalPagos).toFixed(2)}</strong>
                `;
            } else if (totalPagos > totalVenta) {
                alerta.style.display = 'block';
                alerta.className = 'alert alert-info';
                alerta.innerHTML = `
                    <strong>Pago Recibido:</strong> ₡${totalPagos.toFixed(2)} | 
                    <strong>Total Venta:</strong> ₡${totalVenta.toFixed(2)} | 
                    <strong>Vuelto a entregar:</strong> ₡${(totalPagos - totalVenta).toFixed(2)}
                `;
            } else {
                alerta.style.display = 'none';
            }
        };
    }
}

// Función para mostrar alertas con SweetAlert
function mostrarAlertaProducto(mensaje, tipo = 'warning') {
    Swal.fire({
        icon: tipo,
        title: tipo === 'error' ? 'Error' : tipo === 'success' ? 'Éxito' : 'Atención',
        text: mensaje,
        confirmButtonColor: tipo === 'error' ? '#d33' : '#28a745'
    });
}

// Función para seleccionar producto desde el modal
function seleccionarProducto(codigo, nombre, precio, porcentajeImpuesto) {
    const inputBuscar = document.getElementById('inputBuscarProducto');
    inputBuscar.value = `${codigo} - ${nombre}`;
    inputBuscar.setAttribute('data-codigo', codigo);
    inputBuscar.setAttribute('data-porcentaje-impuesto', porcentajeImpuesto);
    
    // Llenar campos ocultos
    document.getElementById('inputPrecioUnitario').value = precio;
    
    // Cerrar modal
    const modalElement = document.getElementById('modalSeleccionProductos');
    if (modalElement) {
        const modalInstance = bootstrap.Modal.getInstance(modalElement) || new bootstrap.Modal(modalElement);
        modalInstance.hide();
    }
    
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

// FUNCIÓN PRINCIPAL: Buscar producto por código y agregarlo automáticamente
async function buscarYAgregarProductoPorCodigo() {
    const inputBuscar = document.getElementById('inputBuscarProducto');
    const codigo = inputBuscar.value.trim();
    
    if (!codigo) {
        mostrarAlertaProducto('Ingrese un código de producto', 'warning');
        return;
    }

    try {
        // Indicador visual durante búsqueda
        inputBuscar.style.borderColor = '#28a745';
        inputBuscar.style.background = '#e8f5e8';
        inputBuscar.disabled = true;
        
        // Buscar el producto por código
        const response = await fetch(`/Operaciones/Ventas/Upsert?handler=ProductoInfo&codigo=${encodeURIComponent(codigo)}`);
        const data = await response.json();
        
        if (data.success && data.producto) {
            // Llenar campos con información del producto
            inputBuscar.value = `${data.producto.codigo}`;
            inputBuscar.setAttribute('data-nombre-producto', data.producto.nombre);
            inputBuscar.setAttribute('data-codigo', data.producto.codigo);
            inputBuscar.setAttribute('data-porcentaje-impuesto', data.producto.porcentajeImpuesto);
            
            // Llenar campos ocultos con valores por defecto
            document.getElementById('inputCantidad').value = '1';
            document.getElementById('inputPrecioUnitario').value = data.producto.precio;
            document.getElementById('inputDescuento').value = '0';
            
            // Validar existencias y agregar automáticamente
            if (data.producto.existencia > 0) {
                const resultadoAgregar = await agregarProductoAutomaticamente();
                
                if (resultadoAgregar) {
                    Swal.fire({
                        toast: true,
                        position: 'top-end',
                        icon: 'success',
                        title: `✅ ${data.producto.nombre} agregado`,
                        text: `Cant: 1 | Precio: ₡${data.producto.precio.toFixed(2)} | Stock: ${data.producto.existencia}`,
                        showConfirmButton: false,
                        timer: 2000,
                        timerProgressBar: true
                    });
                }
            } else {
                Swal.fire({
                    toast: true,
                    position: 'top-end',
                    icon: 'warning',
                    title: '⚠️ Sin existencias',
                    text: `${data.producto.nombre} no tiene stock disponible`,
                    showConfirmButton: false,
                    timer: 3000
                });
                
                setTimeout(() => {
                    limpiarCamposProducto();
                }, 2000);
            }
            
        } else {
            // Producto no encontrado
            Swal.fire({
                toast: true,
                position: 'top-end',
                icon: 'warning',
                title: '❌ Código no encontrado',
                text: codigo,
                showConfirmButton: false,
                timer: 2500
            });
            
            // Abrir modal automáticamente después de un momento
            setTimeout(() => {
                const btnBuscar = document.getElementById('btnBuscarProducto');
                if (btnBuscar) {
                    btnBuscar.click();
                }
            }, 1000);
            
            // Limpiar campo para siguiente búsqueda
            setTimeout(() => {
                inputBuscar.value = '';
                inputBuscar.focus();
            }, 1500);
        }
        
    } catch (error) {
        console.error('Error buscando producto:', error);
        Swal.fire({
            toast: true,
            position: 'top-end',
            icon: 'error',
            title: 'Error de conexión',
            showConfirmButton: false,
            timer: 2000
        });
    } finally {
        // Restaurar campo
        inputBuscar.style.background = '';
        inputBuscar.style.borderColor = '';
        inputBuscar.disabled = false;
    }
}

// Función para agregar producto automáticamente
function agregarProductoAutomaticamente() {
    // Esperar un poco para asegurar que VentaManager esté disponible
    return new Promise((resolve) => {
        const intentarAgregar = () => {
            const inputBuscar = document.getElementById('inputBuscarProducto');
            const codigoProducto = inputBuscar.getAttribute('data-codigo');
            const nombreProducto = inputBuscar.getAttribute('data-nombre-producto');
            const cantidad = parseInt(document.getElementById('inputCantidad').value) || 1;
            const precioUnitario = parseFloat(document.getElementById('inputPrecioUnitario').value) || 0;
            const porcentajeDescuento = parseFloat(document.getElementById('inputDescuento').value) || 0;
            const porcentajeImpuesto = parseFloat(inputBuscar.getAttribute('data-porcentaje-impuesto')) || 0;

            if (!codigoProducto || !nombreProducto || precioUnitario <= 0 || cantidad <= 0) {
                console.log('Validación fallida:', { codigoProducto, nombreProducto, precioUnitario, cantidad });
                resolve(false);
                return;
            }

            try {
                // Verificar si VentaManager está disponible
                if (typeof VentaManager !== 'undefined' && VentaManager.agregarProducto) {
                    // Llenar los campos con los datos correctos usando jQuery
                    $('#inputBuscarProducto').val(nombreProducto).data('codigo', codigoProducto);
                    $('#inputCantidad').val(cantidad);
                    $('#inputPrecioUnitario').val(precioUnitario);
                    $('#inputDescuento').val(porcentajeDescuento);
                    $('#inputBuscarProducto').data('porcentajeImpuesto', porcentajeImpuesto);
                    
                    // Llamar al método del VentaManager de la página
                    VentaManager.agregarProducto();
                    
                    console.log('Producto agregado usando VentaManager');
                    resolve(true);
                    return;
                }
                
                console.warn('VentaManager no disponible, usando fallback');
                resolve(false);
                
            } catch (error) {
                console.error('Error agregando producto automáticamente:', error);
                resolve(false);
            }
        };

        // Si VentaManager no está disponible inmediatamente, esperar un poco
        if (typeof VentaManager === 'undefined') {
            setTimeout(intentarAgregar, 100);
        } else {
            intentarAgregar();
        }
    });
}

// También actualizar la función manual para usar la misma lógica
async function agregarProductoManualmente() {
    const inputBuscar = document.getElementById('inputBuscarProducto');
    const codigoBusqueda = inputBuscar.value.trim();
    
    if (!codigoBusqueda) {
        mostrarAlertaProducto('Ingrese un código de producto', 'warning');
        inputBuscar.focus();
        return;
    }
    
    try {
        // Siempre buscar el producto para asegurar datos frescos
        const response = await fetch(`/Operaciones/Ventas/Upsert?handler=ProductoInfo&codigo=${encodeURIComponent(codigoBusqueda)}`);
        const data = await response.json();
        
        if (data.success && data.producto) {
            // Verificar existencias primero
            if (data.producto.existencia <= 0) {
                Swal.fire({
                    toast: true,
                    position: 'top-end',
                    icon: 'warning',
                    title: '⚠️ Sin existencias',
                    text: `${data.producto.nombre} no tiene stock disponible`,
                    showConfirmButton: false,
                    timer: 3000
                });
                return;
            }
            
            // Establecer temporalmente los datos del producto (sin activar el input event)
            const originalValue = inputBuscar.value;
            
            // Deshabilitar temporalmente el input listener
            inputBuscar.removeEventListener('input', limpiarDatosInput);
            
            // Establecer los datos
            inputBuscar.setAttribute('data-nombre-producto', data.producto.nombre);
            inputBuscar.setAttribute('data-codigo', data.producto.codigo);
            inputBuscar.setAttribute('data-porcentaje-impuesto', data.producto.porcentajeImpuesto);
            
            // Llenar campos con valores del producto
            document.getElementById('inputCantidad').value = document.getElementById('inputCantidad').value || '1';
            document.getElementById('inputPrecioUnitario').value = data.producto.precio;
            document.getElementById('inputDescuento').value = document.getElementById('inputDescuento').value || '0';
            
            // Restaurar el input listener
            setTimeout(() => {
                inputBuscar.addEventListener('input', limpiarDatosInput);
            }, 100);
            
            // Usar la función automática
            const resultado = await agregarProductoAutomaticamente();
            
            if (resultado) {
                Swal.fire({
                    toast: true,
                    position: 'top-end',
                    icon: 'success',
                    title: `✅ Producto agregado`,
                    text: `${data.producto.nombre}`,
                    showConfirmButton: false,
                    timer: 2000
                });
            } else {
                Swal.fire({
                    toast: true,
                    position: 'top-end',
                    icon: 'error',
                    title: 'Error al agregar producto',
                    showConfirmButton: false,
                    timer: 2000
                });
            }
        } else {
            mostrarAlertaProducto('Producto no encontrado con el código ingresado', 'warning');
            inputBuscar.focus();
        }
    } catch (error) {
        console.error('Error buscando producto:', error);
        Swal.fire({
            toast: true,
            position: 'top-end',
            icon: 'error',
            title: 'Error de conexión',
            showConfirmButton: false,
            timer: 2000
        });
    }
}

// Función separada para el input listener
function limpiarDatosInput(e) {
    if (e.inputType === 'insertText' || e.inputType === 'deleteContentBackward') {
        this.removeAttribute('data-codigo');
        this.removeAttribute('data-nombre-producto');
        this.setAttribute('data-porcentaje-impuesto', '0');
    }
}

// Función para limpiar campos del producto
function limpiarCamposProducto() {
    const inputBuscar = document.getElementById('inputBuscarProducto');
    inputBuscar.value = '';
    inputBuscar.removeAttribute('data-codigo');
    inputBuscar.setAttribute('data-porcentaje-impuesto', '0');
    
    document.getElementById('inputCantidad').value = '1';
    document.getElementById('inputPrecioUnitario').value = '';
    document.getElementById('inputDescuento').value = '0';
    
    // Volver focus al campo principal para siguiente búsqueda
    inputBuscar.focus();
}