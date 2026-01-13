using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOHPO.Models
{
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        private DateTime _fechaCreacion;
        [Required(ErrorMessage = "La fecha de creación es obligatoria")]
        [DisplayName("Fecha de creación")]
        public DateTime Fecha_Creacion 
        { 
            get => _fechaCreacion;
            set => _fechaCreacion = new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);
        }

        private DateTime _fechaEntrega;
        [Required(ErrorMessage = "La fecha de entrega es obligatoria")]
        [DisplayName("Fecha de entrega")]
        public DateTime Fecha_Entrega 
        { 
            get => _fechaEntrega;
            set => _fechaEntrega = new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);
        }

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
        public ICollection<Pedido_Metodo_Pago> Pedido_Metodo_Pagos { get; set; } = new List<Pedido_Metodo_Pago>();
    }
}