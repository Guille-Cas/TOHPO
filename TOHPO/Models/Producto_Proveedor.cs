using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOHPO.Models
{
    public class Producto_Proveedor
    {
        [Key, Column(Order = 0)]
        [ForeignKey("Producto")]
        public string Codigo_Producto { get; set; }
        public Producto Producto { get; set; }
        [ForeignKey("Proveedor")]
        [Key, Column(Order = 1)]
        public int Id_Proveedor { get; set; }
        public Proveedor Proveedor { get; set; }
    }
}
