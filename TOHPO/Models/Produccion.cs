using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOHPO.Models
{
    public class Produccion
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        [DisplayName("Fecha")]
        public DateTime Fecha { get; set; }

        [DisplayName("Obra")]
        [StringLength(100)]
        public string Obra { get; set; }

        [DisplayName("Descripción")]
        [StringLength(500)]
        public string Descripcion { get; set; }

        [DisplayName("Fecha planeada")]
        public DateTime Fecha_Planeada { get; set; }

        public bool Estado { get; set; }

        // Navegación
        public ICollection<Produccion_Detalle> Produccion_Detalles { get; set; } = new List<Produccion_Detalle>();
    }
}