using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOHPO.Models
{
    public class Impuesto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [DisplayName("Descripción")]
        public string Descripcion { get; set; }


        [Required(ErrorMessage = "El Porcentaje es obligatorio")]
        [Range(1, 100, ErrorMessage = "El porcentaje debe estar entre 1 y 100.")]
        [Column(TypeName = "decimal(5,2)")] 
        public double Porcentaje { get; set; }

        public bool Estado { get; set; }
    }
}
