using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TOHPO.Data;

namespace TOHPO.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly AppDbContext _context;

        public IndexModel(ILogger<IndexModel> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public ResumenPedidosDto? ResumenPedidos { get; set; }

        public async Task OnGetAsync()
        {
            await CargarResumenPedidos();
        }

        private async Task CargarResumenPedidos()
        {
            try
            {
                var hoy = DateTime.Now.Date;
                
                ResumenPedidos = new ResumenPedidosDto
                {
                    PedidosPendientes = await _context.Pedido
                        .CountAsync(p => !p.Estado),
                    
                    PedidosCompletados = await _context.Pedido
                        .CountAsync(p => p.Estado && p.Fecha_Entrega.Date == hoy),
                    
                    TotalPendiente = await _context.Pedido
                        .Where(p => !p.Estado)
                        .SumAsync(p => p.Saldo),
                    
                    PedidosVencidos = await _context.Pedido
                        .CountAsync(p => !p.Estado && p.Fecha_Entrega.Date < hoy)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar resumen de pedidos");
                ResumenPedidos = null;
            }
        }
    }

    public class ResumenPedidosDto
    {
        public int PedidosPendientes { get; set; }
        public int PedidosCompletados { get; set; }
        public decimal TotalPendiente { get; set; }
        public int PedidosVencidos { get; set; }
    }
}
