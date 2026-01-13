using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TOHPO.Data;
using TOHPO.Models;

namespace TOHPO.Pages.Operaciones.Inventario
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Models.Inventario> Inventario { get; set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.Inventario != null)
            {
                Inventario = await _context.Inventario
                    .Include(i => i.Producto)
                    .Where(i => i.Estado == true)
                    .OrderBy(i => i.Producto.Descripcion)
                    .ToListAsync();
            }
        }
    }
}