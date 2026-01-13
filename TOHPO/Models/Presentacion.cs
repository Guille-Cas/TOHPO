using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TOHPO.Models.Enums;

namespace TOHPO.Models
{
    public class Presentacion
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        [DisplayName("Cantidad:")]
        public double Cantidad { get; set; }
        
        [Required(ErrorMessage = "La unidad de medida es obligatoria")]
        [DisplayName("Unidad de medida:")]
        public Unidad_Medida Unidad_Medida { get; set; }
        
        public bool Estado { get; set; } = true;
    }
}
