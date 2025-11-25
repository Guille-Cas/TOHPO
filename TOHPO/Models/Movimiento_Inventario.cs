using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOHPO.Models
{
    public class Movimiento_Inventario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [DisplayName("Cantidad")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El motivo es obligatorio")]
        [DisplayName("Motivo")]
        [StringLength(150)]
        public string Motivo { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        [DisplayName("Fecha")]
        public DateTime Fecha { get; set; }

        // Relaciones
        [ForeignKey("Inventario")]
        public int Id_Inventario { get; set; }
        public Inventario Inventario { get; set; }
    }
}