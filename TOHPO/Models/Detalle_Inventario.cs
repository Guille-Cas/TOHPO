using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOHPO.Models
{
    public class Detalle_Inventario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        [DisplayName("Estado")]
        [StringLength(50)]
        public string Estado { get; set; }

        [Required(ErrorMessage = "La cantidad de materia prima es obligatoria")]
        [DisplayName("Cantidad materia prima")]
        public int Cantidad_Materia_Prima { get; set; }

        // Relaciones
        [ForeignKey("Inventario")]
        public int Id_Inventario { get; set; }
        public Inventario Inventario { get; set; }
    }
}