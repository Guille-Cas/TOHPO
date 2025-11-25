using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOHPO.Models
{
    public class Produccion_Detalle
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La cantidad de productos es obligatoria")]
        [DisplayName("Cantidad productos")]
        public int Cantidad_Productos { get; set; }

        [Required(ErrorMessage = "La cantidad de preparación es obligatoria")]
        [DisplayName("Cantidad preparación")]
        public int Cantidad_Preparacion { get; set; }

        // Relaciones
        [ForeignKey("Produccion")]
        public int Id_Produccion { get; set; }
        public Produccion Produccion { get; set; }

        [ForeignKey("Producto")]
        public string Codigo_Producto { get; set; }
        public Producto Producto { get; set; }
    }
}