using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOHPO.Models
{
    public class Venta
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        [DisplayName("Fecha")]
        public DateTime Fecha { get; set; }

        [DisplayName("Concepto")]
        [StringLength(500)]
        public string Concepto { get; set; }

        [DisplayName("Costo total gravado")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Costo_Total_Gravado { get; set; }

        [DisplayName("IVA")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Iva { get; set; }

        [DisplayName("Total")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public DateTime Hora { get; set; }

        // Relaciones
        [ForeignKey("Cliente")]
        public int Id_Cliente { get; set; }
        public Cliente Cliente { get; set; }

        [Display(Name = "Agente de Ventas")]

        // Navegación
        public ICollection<Detalle_Venta> Detalle_Ventas { get; set; } = new List<Detalle_Venta>();
        public ICollection<Venta_Metodo_Pago> Venta_Metodo_Pagos { get; set; } = new List<Venta_Metodo_Pago>();
    }
}