using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOHPO.Models
{
    public class Pedido_Metodo_Pago
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        // Relaciones
        [ForeignKey("Pedido")]
        public int Id_Pedido { get; set; }
        public Pedido Pedido { get; set; }

        [ForeignKey("Metodo_Pago")]
        public int Id_Metodo_Pago { get; set; }
        public Metodo_Pago Metodo_Pago { get; set; }
    }
}