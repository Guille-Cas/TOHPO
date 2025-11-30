using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace TOHPO.Models
{
    public class Proveedor
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(30, ErrorMessage = "El nombre no puede exceder 30 caracteres")]
        [DisplayName("Nombre")]
        public string Nombre { get; set; }

        [DisplayName("Teléfono")]
        [RegularExpression(@"^\d{4}-\d{4}$", ErrorMessage = "Formato inválido. Use 0000-0000")]
        public string Telefono { get; set; }

        [DisplayName("Correo")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
        public string Correo_Electronico { get; set; }
        
        [DisplayName("Dirección")]
        public string Direccion { get; set; }
        
        public bool Estado { get; set; }
    }
}
