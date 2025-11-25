using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TOHPO.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El primer apellido es obligatorio")]
        [DisplayName("Primer apellido")]
        public string Primer_Apellido { get; set; }

        [Required(ErrorMessage = "El segundo apellido es obligatorio")]
        [DisplayName("Segundo apellido")]
        public string Segundo_Apellido { get; set; }

        [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
        [DisplayName("Correo")]
        public string Correo_Electronico { get; set; }


        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [RegularExpression(@"^\d{4}-\d{4}$", ErrorMessage = "Formato inválido. Use 0000-0000")]
        [DisplayName("Teléfono")]
        public string Telefono { get; set; }


        [Required(ErrorMessage = "La cédula es obligatoria")]
        [DisplayName("Cédula")]
        public String Cedula { get; set; }
    }
}
