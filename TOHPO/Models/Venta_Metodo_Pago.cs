using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOHPO.Models
{
    public class Venta_Metodo_Pago
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Monto")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        // Relaciones
        [ForeignKey("Venta")]
        public int Id_Venta { get; set; }
        public Venta Venta { get; set; }

        [ForeignKey("Metodo_Pago")]
        public int Id_Metodo_Pago { get; set; }
        public Metodo_Pago Metodo_Pago { get; set; }
    }
}