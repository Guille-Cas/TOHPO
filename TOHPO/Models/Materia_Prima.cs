using TOHPO.Models.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TOHPO.Models
{
    public class Materia_Prima
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [DisplayName("Descripción:")]
        public string Descripcion { get; set; }
        public bool Estado { get; set; }

        [Required(ErrorMessage = "La unidad de medida es obligatoria")]
        [DisplayName("Unidad de medida:")]
        public Unidad_Medida Unidad_Medida { get; set; }
    }
}
