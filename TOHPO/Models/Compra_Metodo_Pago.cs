using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOHPO.Models
{
    public class Compra_Metodo_Pago
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [DisplayName("Monto")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        // Relaciones
        [ForeignKey("Compra")]
        public int Id_Compra { get; set; }
        public Compra Compra { get; set; }

        [ForeignKey("Metodo_Pago")]
        public int Id_Metodo_Pago { get; set; }
        public Metodo_Pago Metodo_Pago { get; set; }
    }
}