using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOHPO.Models
{
    public class Compra
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        [DisplayName("Fecha")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "El número de factura es obligatorio")]
        [DisplayName("Número de factura")]
        [StringLength(50)]
        public string Numero_Factura { get; set; }

        [DisplayName("Total")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "El total debe ser mayor o igual a 0")]
        public decimal Total { get; set; }

        [DisplayName("IVA")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "El IVA debe ser mayor o igual a 0")]
        public decimal Iva { get; set; }

        [DisplayName("Gran total")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "El gran total debe ser mayor o igual a 0")]
        public decimal Gran_Total { get; set; }

        public bool Estado { get; set; }

        // Relaciones
        [Required(ErrorMessage = "Debe seleccionar un proveedor")]
        [DisplayName("Proveedor")]
        [ForeignKey("Proveedor")]
        public int Id_Proveedor { get; set; }
        public Proveedor? Proveedor { get; set; }

        // Navegación
        public ICollection<Compra_Detalle> Compra_Detalles { get; set; } = new List<Compra_Detalle>();
        public ICollection<Compra_Metodo_Pago> Compra_Metodo_Pagos { get; set; } = new List<Compra_Metodo_Pago>();
    }
}