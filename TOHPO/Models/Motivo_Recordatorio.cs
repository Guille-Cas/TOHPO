using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TOHPO.Models
{
    public class Motivo_Recordatorio
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [DisplayName("Descripción")]
        public string Descripcion { get; set; }
    }
}
