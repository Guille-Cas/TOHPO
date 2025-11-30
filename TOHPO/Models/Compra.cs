using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOHPO.Models
{
    public class Compra
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        [DisplayName("Fecha")]
        public DateTime Fecha { get; set; } = DateTime.Now.Date;

        [Required(ErrorMessage = "La hora es obligatoria")]
        [DisplayName("Hora")]
        public DateTime Hora { get; set; } = DateTime.Now;

        [DisplayName("Concepto")]
        [StringLength(500, ErrorMessage = "El concepto no puede exceder 500 caracteres")]
        public string? Concepto { get; set; }

        [Required(ErrorMessage = "El costo total grabado es obligatorio")]
        [DisplayName("Costo Total Grabado")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "El costo total grabado no puede ser negativo")]
        public decimal Costo_Total_Grabado { get; set; }

        [Required(ErrorMessage = "El IVA es obligatorio")]
        [DisplayName("IVA")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "El IVA no puede ser negativo")]
        public decimal Iva { get; set; }

        [Required(ErrorMessage = "El total es obligatorio")]
        [DisplayName("Total")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "El total no puede ser negativo")]
        public decimal Total { get; set; }

        // Relaciones
        [Required(ErrorMessage = "El proveedor es obligatorio")]
        [ForeignKey("Proveedor")]
        public int Id_Proveedor { get; set; }
        public Proveedor Proveedor { get; set; }

        // Colecciones de navegación
        public ICollection<Compra_Detalle> Compra_Detalles { get; set; } = new List<Compra_Detalle>();
        public ICollection<Compra_Metodo_Pago> Compra_Metodo_Pagos { get; set; } = new List<Compra_Metodo_Pago>();
    }
}