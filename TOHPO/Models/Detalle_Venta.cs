using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOHPO.Models
{
    public class Detalle_Venta
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [DisplayName("Cantidad")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El precio unitario es obligatorio")]
        [DisplayName("Precio unitario")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio_Unitario { get; set; }

        [DisplayName("Porcentaje descuento")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Porcentaje_Descuento { get; set; }

        [DisplayName("Monto descuento")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto_Descuento { get; set; }

        [DisplayName("Monto impuesto")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto_Impuesto { get; set; }

        [DisplayName("Subtotal")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        // Relaciones
        [ForeignKey("Venta")]
        public int Id_Venta { get; set; }
        public Venta Venta { get; set; }

        [ForeignKey("Producto")]
        public string Codigo_Producto { get; set; }
        public Producto Producto { get; set; }
    }
}