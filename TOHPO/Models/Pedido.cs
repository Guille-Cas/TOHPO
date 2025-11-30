using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOHPO.Models
{
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha de creación es obligatoria")]
        [DisplayName("Fecha de creación")]
        public DateTime Fecha_Creacion { get; set; }

        [Required(ErrorMessage = "La fecha de entrega es obligatoria")]
        [DisplayName("Fecha de entrega")]
        public DateTime Fecha_Entrega { get; set; }

        [DisplayName("Abono")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "El abono no puede ser negativo")]
        public decimal Abono { get; set; }

        [DisplayName("Saldo")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "El saldo no puede ser negativo")]
        public decimal Saldo { get; set; }

        [DisplayName("Total")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "El total no puede ser negativo")]
        public decimal Total { get; set; }

        public bool Estado { get; set; }

        // Relaciones
        [ForeignKey("Cliente")]
        public int Id_Cliente { get; set; }
        public Cliente Cliente { get; set; }

        // Navegación
        public ICollection<Pedido_Detalle> Pedido_Detalles { get; set; } = new List<Pedido_Detalle>();
    }
}