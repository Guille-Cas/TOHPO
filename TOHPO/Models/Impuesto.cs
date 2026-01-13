using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOHPO.Models
{
    public class Impuesto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [DisplayName("Descripción:")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "La descripción solo debe contener texto, no números ni símbolos")]
        [StringLength(100, ErrorMessage = "La descripción no puede exceder 100 caracteres")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El Porcentaje es obligatorio")]
        [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100.")]
        [Column(TypeName = "decimal(5,2)")]
        [DisplayName("Porcentaje:")]
        public decimal Porcentaje { get; set; }

        public bool Estado { get; set; }

        // Navegación inversa
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
