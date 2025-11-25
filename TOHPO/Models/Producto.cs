using TOHPO.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOHPO.Models
{
    public class Producto
    {
        [Key]
        [DisplayName("Código de referencia")]
        [Required(ErrorMessage = "El código de referencia es obligatorio")]
        public string CodigoReferencia { get; set; }

        [DisplayName("Código de barra")]
        public string Codigo_Barra { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [DisplayName("Descripción")]
        public string Descripcion { get; set; }

        public bool Es_Materia_Prima { get; set; }

        public bool Es_De_Terceros { get; set; }

        [Required(ErrorMessage = "La unidad de medida es obligatoria")]
        [DisplayName("Unidad de medida")]
        public Unidad_Medida Unidad_Medida { get; set; }

        [DisplayName("Tiempo de vida")]
        public int Tiempo_De_Vida { get; set; }

        public bool Estado { get; set; }


        [ForeignKey("Categoria")]
        public int Id_Categoria { get; set; }
        public Categoria Categoria { get; set; }


        [ForeignKey("Materia_Prima")]
        public int Id_Materia_Prima { get; set; }

        public Materia_Prima Materia_Prima { get; set; }
        [ForeignKey("Presentacion")]

        public int Id_Presentacion { get; set; }
        public Presentacion Presentacion { get; set; }

        [ForeignKey("Impuesto")]
        public int Id_Impuesto { get; set; }
        public Impuesto Impuesto { get; set; }

    }
}
