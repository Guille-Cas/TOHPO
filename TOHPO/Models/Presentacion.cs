using TOHPO.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace TOHPO.Models
{
    public class Presentacion
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "La cantidad es obligatoria")]
        public double Cantidad { get; set; }
        [Required(ErrorMessage = "La unidad de medida es obligatoria")]
        public Unidad_Medida Unidad_Medida { get; set; }
        public bool Estado { get; set; }
  
    }
}
