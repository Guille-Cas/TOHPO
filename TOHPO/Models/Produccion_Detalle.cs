using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOHPO.Models
{
    public class Produccion_Detalle
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El ID de producción es obligatorio")]
        [DisplayName("Producción")]
        public int Id_Produccion { get; set; }

        [Required(ErrorMessage = "El ID de receta es obligatorio")]
        [DisplayName("Receta")]
        public int Id_Receta { get; set; }

        [Required(ErrorMessage = "El código de producto es obligatorio")]
        [DisplayName("Código de Producto")]
        public string Codigo_Producto { get; set; }

        [Required(ErrorMessage = "La cantidad programada es obligatoria")]
        [DisplayName("Cantidad Programada")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad programada debe ser mayor a 0")]
        public double Cantidad_Programada { get; set; }

        [DisplayName("Cantidad Producida")]
        [Range(0, double.MaxValue, ErrorMessage = "La cantidad producida no puede ser negativa")]
        public double Cantidad_Producida { get; set; }

        [DisplayName("Estado")]
        public bool Estado { get; set; }

        [DisplayName("Fecha Inicio")]
        public DateTime? Fecha_Inicio { get; set; }

        [DisplayName("Fecha Fin")]
        public DateTime? Fecha_Fin { get; set; }

        [DisplayName("Observaciones")]
        [StringLength(500)]
        public string? Observaciones { get; set; }

        // Relaciones de navegación
        [ForeignKey("Id_Produccion")]
        public Produccion Produccion { get; set; }

        [ForeignKey("Id_Receta")]
        public Receta Receta { get; set; }

        [ForeignKey("Codigo_Producto")]
        public Producto Producto { get; set; }
    }
}