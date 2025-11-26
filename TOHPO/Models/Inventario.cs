using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOHPO.Models
{
    public class Inventario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [DisplayName("Cantidad")]
        public int Cantidad { get; set; }

        [DisplayName("Existencia")]
        public int Existencia { get; set; }

        [Required(ErrorMessage = "El precio de venta es obligatorio")]
        [DisplayName("Precio de venta")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio de venta debe ser mayor a 0")]
        public decimal Precio_Venta { get; set; }

        [DisplayName("Precio de compra")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio_Compra { get; set; }

        public bool Estado { get; set; }

        // Relaciones
        [ForeignKey("Producto")]
        public string Codigo_Producto { get; set; }
        public Producto Producto { get; set; }

        // Navegación
        public ICollection<Detalle_Inventario> Detalle_Inventarios { get; set; } = new List<Detalle_Inventario>();
    }
}