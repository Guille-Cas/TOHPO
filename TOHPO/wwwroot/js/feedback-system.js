/**
 * Sistema de feedback universal para TOHPO
 * Proporciona funciones para mostrar mensajes de éxito, error, advertencia y validación
 */

class FeedbackSystem {
    constructor() {
        this.initializeSweetAlert();
    }

    // Configurar SweetAlert2 si está disponible
    initializeSweetAlert() {
        if (typeof Swal !== 'undefined') {
            // Configuración global de SweetAlert2
            this.swalDefaults = {
                confirmButtonColor: '#007bff',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Aceptar',
                cancelButtonText: 'Cancelar'
            };
        }
    }

    // Mostrar mensaje de éxito
    success(mensaje, titulo = 'Éxito') {
        this.show('success', mensaje, titulo);
    }

    // Mostrar mensaje de error
    error(mensaje, titulo = 'Error') {
        this.show('error', mensaje, titulo);
    }

    // Mostrar mensaje de advertencia
    warning(mensaje, titulo = 'Advertencia') {
        this.show('warning', mensaje, titulo);
    }

    // Mostrar mensaje informativo
    info(mensaje, titulo = 'Información') {
        this.show('info', mensaje, titulo);
    }

    // Mostrar mensaje genérico
    show(tipo, mensaje, titulo) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: tipo,
                title: titulo,
                text: mensaje,
                timer: tipo === 'success' ? 3000 : null,
                showConfirmButton: tipo !== 'success',
                ...this.swalDefaults
            });
        } else {
            // Fallback a alert nativo
            alert(`${titulo}: ${mensaje}`);
        }
    }

    // Confirmar una acción
    confirm(mensaje, callback, titulo = '¿Está seguro?') {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: titulo,
                text: mensaje,
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                confirmButtonText: 'Sí, continuar',
                ...this.swalDefaults
            }).then((result) => {
                if (result.isConfirmed) {
                    callback();
                }
            });
        } else {
            if (confirm(`${titulo}\n${mensaje}`)) {
                callback();
            }
        }
    }

    // Confirmar eliminación
    confirmDelete(entidad, callback, nombreEntidad = '') {
        const mensaje = nombreEntidad ? 
            `¿Desea eliminar ${entidad} "${nombreEntidad}"?` : 
            `¿Desea eliminar este ${entidad}?`;
        
        this.confirm(mensaje, callback, 'Confirmar eliminación');
    }

    // Mostrar diálogo de desactivación cuando no se puede eliminar
    showDeactivationDialog(mensaje, callback, entidad = 'registro') {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: 'No se puede eliminar',
                text: mensaje,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#ffc107',
                confirmButtonText: `Sí, desactivar ${entidad}`,
                ...this.swalDefaults
            }).then((result) => {
                if (result.isConfirmed) {
                    callback();
                }
            });
        } else {
            const confirmar = confirm(`${mensaje}\n\n¿Desea desactivar el ${entidad} en su lugar?`);
            if (confirmar) {
                callback();
            }
        }
    }

    // Manejar respuesta del servidor
    handleResponse(response, successCallback = null) {
        if (response.success) {
            this.success(response.message);
            if (successCallback) {
                successCallback(response);
            }
        } else {
            switch (response.type) {
                case 'validation':
                    if (response.shouldDeactivate) {
                        // No podemos llamar directamente al callback desde aquí
                        // porque necesitamos el contexto del ID
                        return { action: 'showDeactivationDialog', response: response };
                    } else {
                        this.warning(response.message);
                    }
                    break;
                case 'warning':
                    this.warning(response.message);
                    break;
                case 'error':
                default:
                    this.error(response.message);
                    break;
            }
        }
        return { action: 'handled', response: response };
    }

    // Manejar errores de conexión
    handleConnectionError(error) {
        console.error('Error de conexión:', error);
        this.error('Error de conexión con el servidor. Verifique su conexión a internet.');
    }
}

// Clase para manejar peticiones AJAX de forma consistente
class AjaxHelper {
    constructor(feedbackSystem) {
        this.feedback = feedbackSystem;
    }

    // Obtener token CSRF
    getCSRFToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    }

    // Preparar datos para envío
    prepareData(data, includeToken = true) {
        const formData = new URLSearchParams(data);
        
        if (includeToken) {
            const token = this.getCSRFToken();
            if (token) {
                formData.append('__RequestVerificationToken', token);
            }
        }
        
        return formData.toString();
    }

    // Realizar petición POST
    async post(url, data, successCallback = null) {
        try {
            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: this.prepareData(data)
            });

            const text = await response.text();
            let result;
            
            try {
                result = JSON.parse(text);
            } catch {
                console.error("Respuesta no es JSON válido:", text);
                throw new Error("El servidor no devolvió JSON válido");
            }

            const handleResult = this.feedback.handleResponse(result, successCallback);
            return { result, handleResult };

        } catch (error) {
            this.feedback.handleConnectionError(error);
            throw error;
        }
    }

    // Realizar petición GET
    async get(url) {
        try {
            const response = await fetch(url);
            const text = await response.text();
            
            try {
                return JSON.parse(text);
            } catch {
                console.error("Respuesta no es JSON válido:", text);
                throw new Error("El servidor no devolvió JSON válido");
            }
        } catch (error) {
            this.feedback.handleConnectionError(error);
            throw error;
        }
    }
}

// Instanciar sistema global
const feedback = new FeedbackSystem();
const ajax = new AjaxHelper(feedback);

// Funciones de conveniencia globales
window.showSuccess = (mensaje, titulo) => feedback.success(mensaje, titulo);
window.showError = (mensaje, titulo) => feedback.error(mensaje, titulo);
window.showWarning = (mensaje, titulo) => feedback.warning(mensaje, titulo);
window.showInfo = (mensaje, titulo) => feedback.info(mensaje, titulo);
window.confirmAction = (mensaje, callback, titulo) => feedback.confirm(mensaje, callback, titulo);
window.confirmDelete = (entidad, callback, nombre) => feedback.confirmDelete(entidad, callback, nombre);