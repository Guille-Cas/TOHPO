using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOHPO.Models
{
    public class Receta
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string Descripcion { get; set; }
        [Required(ErrorMessage = "El rendimiento es obligatorio")]
        public double Rendimiento { get; set; }
        [Required(ErrorMessage = "Las instrucciones son obligatorias")]
        public string Instrucciones { get; set; }
        public string Detalle { get; set; }
        public double Cantidad_Empaque { get; set; }
        public DateTime Fecha_Creacion { get; set; }
        public bool Estado { get; set; }

        [ForeignKey("Producto")]
        public string Codigo_Producto { get; set; }
        public Producto Producto { get; set; }

        // Colección de materias primas
        public ICollection<Receta_Materia_Prima> Receta_Materias_Primas { get; set; } = new List<Receta_Materia_Prima>();
    }
}
