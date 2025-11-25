using System.ComponentModel.DataAnnotations;

namespace TOHPO.Models.Enums
{
    public enum TipoRecurrencia
    {
        [Display(Name = "Diario")]
        Diario = 1,

        [Display(Name = "Semanal")]
        Semanal = 2,

        [Display(Name = "Mensual")]
        Mensual = 3,

        [Display(Name = "Anual")]
        Anual = 4
    }
}
