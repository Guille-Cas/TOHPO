// JavaScript para la página de Upsert de Compras - MODO DIRECTO SIMPLIFICADO

document.addEventListener('DOMContentLoaded', function() {
    // Auto-focus en el campo principal de búsqueda
    const inputBuscarProducto = document.getElementById('inputBuscarProducto');
    if (inputBuscarProducto) {
        inputBuscarProducto.focus();
        
        // Configurar búsqueda automática en el campo principal con Enter
        inputBuscarProducto.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                buscarYAgregarProductoPorCodigoCompra();
            }
        });

        // Usar la función separada para limpiar datos
        inputBuscarProducto.addEventListener('input', limpiarDatosInputCompra);
    }
    
    // Modal de productos - si existe
    const modalProductos = document.getElementById('modalSeleccionarProducto');
    const buscadorProducto = document.getElementById('buscarEnModalCompra');
    
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
                if (typeof filtrarProductosCompra === 'function') {
                    filtrarProductosCompra();
                }
            }
        });

        // Búsqueda automática por código de referencia en modal
        buscadorProducto.addEventListener('input', function(e) {
            const inputValue = e.target.value.trim();
            
            if (inputValue.length >= 3) { // Código de referencia suele ser más corto
                setTimeout(() => {
                    buscarProductoPorCodigoEnModalCompra(inputValue);
                }, 100);
            }
        });
    }

    // Actualizar hora automáticamente cada minuto
    actualizarHoraEnTiempoRealCompra();
    setInterval(actualizarHoraEnTiempoRealCompra, 60000);

    // Atajos de teclado para navegación rápida
    document.addEventListener('keydown', function(e) {
        // F2 para abrir selector de productos
        if (e.key === 'F2') {
            e.preventDefault();
            const btnSeleccionarProducto = document.getElementById('btnSeleccionarProducto');
            if (btnSeleccionarProducto) {
                btnSeleccionarProducto.click();
            }
        }
        
        // F3 para agregar producto directamente
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

    // Inicializar funciones de apoyo
    configurarValidacionPagosFlexibleCompra();
    
    // Configurar botón agregar
    const btnAgregar = document.getElementById('btnAgregarProducto');
    if (btnAgregar) {
        btnAgregar.addEventListener('click', function(e) {
            e.preventDefault();
            agregarProductoManualmenteCompra();
        });
    }
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
    const filas = document.querySelectorAll('#tablaProductosModalCompra tr');
    let productoEncontrado = null;

    filas.forEach(fila => {
        const celdaCodigo = fila.querySelector('td:nth-child(2)'); // Asumiendo que el código está en la segunda columna
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

// Función para configurar validación flexible de pagos en compras
function configurarValidacionPagosFlexibleCompra() {
    if (typeof window.actualizarValidacionPagos === 'undefined') {
        window.actualizarValidacionPagos = function() {
            const totalCompra = parseFloat(document.getElementById('totalDisplay').textContent.replace('₡', '').replace(',', '')) || 0;
            const totalPagos = window.metodosPagoData ? window.metodosPagoData.reduce((sum, metodo) => sum + metodo.monto, 0) : 0;
            
            const alerta = document.getElementById('alertaPagos');
            if (!alerta) return;
            
            if (totalPagos < totalCompra) {
                alerta.style.display = 'block';
                alerta.className = 'alert alert-danger';
                alerta.innerHTML = `
                    <strong>¡Atención!</strong> 
                    Total Compra: ₡${totalCompra.toFixed(2)} - 
                    Total Pagos: ₡${totalPagos.toFixed(2)} - 
                    <strong>Faltante: ₡${(totalCompra - totalPagos).toFixed(2)}</strong>
                `;
            } else if (totalPagos > totalCompra) {
                alerta.style.display = 'block';
                alerta.className = 'alert alert-info';
                alerta.innerHTML = `
                    <strong>Pago Realizado:</strong> ₡${totalPagos.toFixed(2)} | 
                    <strong>Total Compra:</strong> ₡${totalCompra.toFixed(2)} | 
                    <strong>Exceso/Crédito:</strong> ₡${(totalPagos - totalCompra).toFixed(2)}
                `;
            } else {
                alerta.style.display = 'none';
            }
        };
    }
}

// Función para mostrar alertas con SweetAlert
function mostrarAlertaProductoCompra(mensaje, tipo = 'warning') {
    Swal.fire({
        icon: tipo,
        title: tipo === 'error' ? 'Error' : tipo === 'success' ? 'Éxito' : 'Atención',
        text: mensaje,
        confirmButtonColor: tipo === 'error' ? '#d33' : '#28a745'
    });
}

// Función para seleccionar producto desde el modal
function seleccionarProductoCompra(codigo, nombre, costo, existencia, impuesto) {
    const inputBuscar = document.getElementById('inputBuscarProducto');
    inputBuscar.value = `${codigo}`;
    inputBuscar.setAttribute('data-codigo', codigo);
    inputBuscar.setAttribute('data-porcentaje-impuesto', impuesto);
    inputBuscar.setAttribute('data-nombre-producto', nombre);
    
    // Llenar campos ocultos
    document.getElementById('inputCostoUnitario').value = costo;
    
    // Cerrar modal
    const modalElement = document.getElementById('modalSeleccionarProducto');
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

// FUNCIÓN PRINCIPAL: Buscar producto por código de referencia y agregarlo automáticamente
async function buscarYAgregarProductoPorCodigoCompra() {
    const inputBuscar = document.getElementById('inputBuscarProducto');
    const codigo = inputBuscar.value.trim();
    
    if (!codigo) {
        mostrarAlertaProductoCompra('Ingrese un código de referencia del producto', 'warning');
        return;
    }

    try {
        // Indicador visual durante búsqueda
        inputBuscar.style.borderColor = '#007bff';
        inputBuscar.style.background = '#e3f2fd';
        inputBuscar.disabled = true;
        
        // Buscar el producto por código de referencia
        const response = await fetch(`/Operaciones/Compras/Upsert?handler=ProductoInfo&codigo=${encodeURIComponent(codigo)}`);
        const data = await response.json();
        
        if (data.success && data.producto) {
            // Llenar campos con información del producto
            inputBuscar.value = `${data.producto.codigo}`;
            inputBuscar.setAttribute('data-nombre-producto', data.producto.nombre);
            inputBuscar.setAttribute('data-codigo', data.producto.codigo);
            inputBuscar.setAttribute('data-porcentaje-impuesto', data.producto.porcentajeImpuesto);
            
            // Llenar campos ocultos con valores por defecto
            document.getElementById('inputCantidad').value = '1';
            document.getElementById('inputCostoUnitario').value = data.producto.costo;
            document.getElementById('inputDescuento').value = '0';
            
            // Agregar automáticamente el producto
            const resultadoAgregar = await agregarProductoAutomaticamenteCompra();
            
            if (resultadoAgregar) {
                Swal.fire({
                    toast: true,
                    position: 'top-end',
                    icon: 'success',
                    title: `✅ ${data.producto.nombre} agregado`,
                    text: `Cant: 1 | Costo: ₡${data.producto.costo.toFixed(2)} | Stock: ${data.producto.existencia || 'N/A'}`,
                    showConfirmButton: false,
                    timer: 2000,
                    timerProgressBar: true
                });
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
                const btnBuscar = document.getElementById('btnSeleccionarProducto');
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

// Función para agregar producto automáticamente en compras
function agregarProductoAutomaticamenteCompra() {
    return new Promise((resolve) => {
        const intentarAgregar = () => {
            const inputBuscar = document.getElementById('inputBuscarProducto');
            const codigoProducto = inputBuscar.getAttribute('data-codigo');
            const nombreProducto = inputBuscar.getAttribute('data-nombre-producto');
            const cantidad = parseInt(document.getElementById('inputCantidad').value) || 1;
            const costoUnitario = parseFloat(document.getElementById('inputCostoUnitario').value) || 0;
            const porcentajeDescuento = parseFloat(document.getElementById('inputDescuento').value) || 0;
            const porcentajeImpuesto = parseFloat(inputBuscar.getAttribute('data-porcentaje-impuesto')) || 0;

            if (!codigoProducto || !nombreProducto || costoUnitario <= 0 || cantidad <= 0) {
                console.log('Validación fallida:', { codigoProducto, nombreProducto, costoUnitario, cantidad });
                resolve(false);
                return;
            }

            try {
                // Verificar si CompraManager está disponible
                if (typeof CompraManager !== 'undefined' && CompraManager.agregarProducto) {
                    // Llenar los campos con los datos correctos usando jQuery
                    $('#inputBuscarProducto').val(nombreProducto).data('codigo', codigoProducto);
                    $('#inputCantidad').val(cantidad);
                    $('#inputCostoUnitario').val(costoUnitario);
                    $('#inputDescuento').val(porcentajeDescuento);
                    $('#inputBuscarProducto').data('porcentajeImpuesto', porcentajeImpuesto);
                    
                    // Llamar al método del CompraManager de la página
                    CompraManager.agregarProducto();
                    
                    console.log('Producto agregado usando CompraManager');
                    resolve(true);
                    return;
                }
                
                console.warn('CompraManager no disponible, usando fallback');
                resolve(false);
                
            } catch (error) {
                console.error('Error agregando producto automáticamente:', error);
                resolve(false);
            }
        };

        // Si CompraManager no está disponible inmediatamente, esperar un poco
        if (typeof CompraManager === 'undefined') {
            setTimeout(intentarAgregar, 100);
        } else {
            intentarAgregar();
        }
    });
}

// Función manual para agregar productos
async function agregarProductoManualmenteCompra() {
    const inputBuscar = document.getElementById('inputBuscarProducto');
    const codigoBusqueda = inputBuscar.value.trim();
    
    if (!codigoBusqueda) {
        mostrarAlertaProductoCompra('Ingrese un código de referencia del producto', 'warning');
        inputBuscar.focus();
        return;
    }
    
    try {
        // Siempre buscar el producto para asegurar datos frescos
        const response = await fetch(`/Operaciones/Compras/Upsert?handler=ProductoInfo&codigo=${encodeURIComponent(codigoBusqueda)}`);
        const data = await response.json();
        
        if (data.success && data.producto) {
            // Establecer temporalmente los datos del producto
            const originalValue = inputBuscar.value;
            
            // Deshabilitar temporalmente el input listener
            inputBuscar.removeEventListener('input', limpiarDatosInputCompra);
            
            // Establecer los datos
            inputBuscar.setAttribute('data-nombre-producto', data.producto.nombre);
            inputBuscar.setAttribute('data-codigo', data.producto.codigo);
            inputBuscar.setAttribute('data-porcentaje-impuesto', data.producto.porcentajeImpuesto);
            
            // Llenar campos con valores del producto
            document.getElementById('inputCantidad').value = document.getElementById('inputCantidad').value || '1';
            document.getElementById('inputCostoUnitario').value = data.producto.costo;
            document.getElementById('inputDescuento').value = document.getElementById('inputDescuento').value || '0';
            
            // Restaurar el input listener
            setTimeout(() => {
                inputBuscar.addEventListener('input', limpiarDatosInputCompra);
            }, 100);
            
            // Usar la función automática
            const resultado = await agregarProductoAutomaticamenteCompra();
            
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
            mostrarAlertaProductoCompra('Producto no encontrado con el código ingresado', 'warning');
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
function limpiarDatosInputCompra(e) {
    if (e.inputType === 'insertText' || e.inputType === 'deleteContentBackward') {
        this.removeAttribute('data-codigo');
        this.removeAttribute('data-nombre-producto');
        this.setAttribute('data-porcentaje-impuesto', '0');
    }
}

// Función para limpiar campos del producto
function limpiarCamposProductoCompra() {
    const inputBuscar = document.getElementById('inputBuscarProducto');
    inputBuscar.value = '';
    inputBuscar.removeAttribute('data-codigo');
    inputBuscar.setAttribute('data-porcentaje-impuesto', '0');
    
    document.getElementById('inputCantidad').value = '1';
    document.getElementById('inputCostoUnitario').value = '';
    document.getElementById('inputDescuento').value = '0';
    
    // Volver focus al campo principal para siguiente búsqueda
    inputBuscar.focus();
}

// Alias para compatibilidad con modal
function filtrarProductosCompra() {
    if (typeof CompraManager !== 'undefined' && CompraManager.filtrarProductos) {
        CompraManager.filtrarProductos();
    }
}
