using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Models;
using TOHPO.Data;

namespace TOHPO.Pages.Configuracion.Materias_Prima
{
    public class UpsertModel : PageModel
    {
        private readonly AppDbContext _context;
        public UpsertModel(AppDbContext context) { _context = context; }

        [BindProperty]
        public Materia_Prima MateriaPrima { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id.HasValue)
            {
                MateriaPrima = await _context.Materia_Prima.FindAsync(id.Value);
                if (MateriaPrima == null) return NotFound();
            }
            else
            {
                MateriaPrima = new Materia_Prima { Estado = true }; // Por defecto activo para nuevas materias primas
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            
            if (MateriaPrima.Id > 0)
            {
                var existente = await _context.Materia_Prima.FindAsync(MateriaPrima.Id);
                if (existente == null) return NotFound();
                
                // Actualizar todas las propiedades
                existente.Descripcion = MateriaPrima.Descripcion;
                existente.Unidad_Medida = MateriaPrima.Unidad_Medida;
                existente.Estado = MateriaPrima.Estado;

                _context.Materia_Prima.Update(existente);
            }
            else
            {
                _context.Materia_Prima.Add(MateriaPrima);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToPage("/Configuracion/Materias_Prima/Index");
        }
    }
}
