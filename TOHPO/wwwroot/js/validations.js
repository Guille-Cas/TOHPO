// Validación para solo texto (sin números)
function validateTextOnly(input) {
    const regex = /^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]*$/;
    if (!regex.test(input.value)) {
        input.setCustomValidity('Solo se permiten letras y espacios');
        return false;
    } else {
        input.setCustomValidity('');
        return true;
    }
}

// Validación para valores no negativos
function validateNonNegative(input) {
    const value = parseFloat(input.value);
    if (value < 0) {
        input.setCustomValidity('No se permiten valores negativos');
        return false;
    } else {
        input.setCustomValidity('');
        return true;
    }
}

// Validación para longitud máxima
function validateMaxLength(input, maxLength) {
    if (input.value.length > maxLength) {
        input.setCustomValidity(`No puede exceder ${maxLength} caracteres`);
        return false;
    } else {
        input.setCustomValidity('');
        return true;
    }
}

// Aplicar validaciones en tiempo real
document.addEventListener('DOMContentLoaded', function() {
    // Para campos de descripción de impuestos
    const impuestoDescripcion = document.querySelector('input[name="Impuesto.Descripcion"]');
    if (impuestoDescripcion) {
        impuestoDescripcion.addEventListener('input', function() {
            validateTextOnly(this);
        });
    }

    // Para campos de nombre de proveedor
    const proveedorNombre = document.querySelector('input[name="Proveedor.Nombre"]');
    if (proveedorNombre) {
        proveedorNombre.addEventListener('input', function() {
            validateMaxLength(this, 30);
        });
    }

    // Para campos monetarios y de cantidad
    const numericalFields = document.querySelectorAll('input[type="number"], input[name*="Total"], input[name*="Abono"], input[name*="Saldo"], input[name*="Precio"], input[name*="Cantidad"]');
    numericalFields.forEach(function(field) {
        field.addEventListener('input', function() {
            validateNonNegative(this);
        });
    });
});