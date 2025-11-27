using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TOHPO.Models.Enums;

namespace TOHPO.Models
{
    public class Receta_Materia_Prima
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El ID de receta es obligatorio")]
        [DisplayName("Receta")]
        public int Id_Receta { get; set; }

        [Required(ErrorMessage = "El ID de materia prima es obligatorio")]
        [DisplayName("Materia Prima")]
        public int Id_Materia_Prima { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [DisplayName("Cantidad Requerida")]
        [Range(0.001, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal Cantidad_Requerida { get; set; }

        [DisplayName("Unidad de Medida")]
        public Unidad_Medida Unidad_Medida { get; set; }

        [DisplayName("Observaciones")]
        [StringLength(200)]
        public string? Observaciones { get; set; }

        [DisplayName("Estado")]
        public bool Estado { get; set; } = true;

        // Relaciones de navegación
        [ForeignKey("Id_Receta")]
        public Receta Receta { get; set; }

        [ForeignKey("Id_Materia_Prima")]
        public Materia_Prima Materia_Prima { get; set; }
    }
}