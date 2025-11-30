using TOHPO.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace TOHPO.Models
{
    public class Presentacion
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public double Cantidad { get; set; }
        
        [Required(ErrorMessage = "La unidad de medida es obligatoria")]
        public Unidad_Medida Unidad_Medida { get; set; }
        
        public bool Estado { get; set; } = true;
    }
}
