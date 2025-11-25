using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TOHPO.Models.Enums;

namespace TOHPO.Models
{
    public class Recordatorio
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha y hora es obligatoria")]
        public DateTime Fecha_Hora { get; set; }

        public string? Detalles { get; set; }

        [ForeignKey("Cliente")]
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        [ForeignKey("Motivo_Recordatorio")]
        public int Motivo_RecordatorioId { get; set; }
        public Motivo_Recordatorio? Motivo_Recordatorio { get; set; }

        // Campos de recurrencia
        public bool EsRecurrente { get; set; } = false;

        [Display(Name = "Tipo de recurrencia")]
        public TipoRecurrencia? TipoRecurrencia { get; set; }

        [Display(Name = "Intervalo de recurrencia")]
        public int IntervaloRecurrencia { get; set; } = 1;

        [Display(Name = "Fecha de fin de recurrencia")]
        public DateTime? FechaFinRecurrencia { get; set; }

        [Display(Name = "Número máximo de repeticiones")]
        public int? MaximoRepeticiones { get; set; }

        // Referencia al recordatorio padre (para recordatorios generados por recurrencia)
        [ForeignKey("RecordatorioPadre")]
        public int? RecordatorioPadreId { get; set; }
        public Recordatorio? RecordatorioPadre { get; set; }

        // Colección de recordatorios hijo (generados por recurrencia)
        public ICollection<Recordatorio> RecordatoriosHijo { get; set; } = new List<Recordatorio>();
    }
}
