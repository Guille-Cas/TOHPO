using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOHPO.Models
{
    public class Inventario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [DisplayName("Cantidad")]
        public int Cantidad { get; set; }

        public bool Estado { get; set; }

        // Relaciones
        [ForeignKey("Producto")]
        public string Codigo_Producto { get; set; }
        public Producto Producto { get; set; }

        // Navegación
        public ICollection<Detalle_Inventario> Detalle_Inventarios { get; set; } = new List<Detalle_Inventario>();
    }
}