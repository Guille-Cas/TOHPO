using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOHPO.Models
{
    public class Agente_Ventas
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        [DisplayName("Teléfono")]
        [RegularExpression(@"^\d{4}-\d{4}$", ErrorMessage = "Formato inválido. Use 0000-0000")]
        [Required(ErrorMessage = "El teléfono es obligatorio")]
        public string Telefono { get; set; }

        [DisplayName("Correo")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
        public string Correo_Electronico { get; set; }

        public bool Estado { get; set; }

        [ForeignKey("Proveedor")]
        public int Id_Proveedor { get; set; }
        public Proveedor Proveedor { get; set; }
    }
}
