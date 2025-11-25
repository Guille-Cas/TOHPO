using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOHPO.Models
{
    public class Pedido_Detalle
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

        // Relaciones
        [ForeignKey("Pedido")]
        public int Id_Pedido { get; set; }
        public Pedido Pedido { get; set; }

        [ForeignKey("Producto")]
        public string Codigo_Producto { get; set; }
        public Producto Producto { get; set; }
    }
}