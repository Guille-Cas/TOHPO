using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TOHPO.Models;
using TOHPO.Data;
using Microsoft.EntityFrameworkCore;
using TOHPO.Models.Enums;

namespace TOHPO.Pages.Recordatorios
{
    public class UpsertModel : PageModel
    {
        private readonly AppDbContext _context;
        public UpsertModel(AppDbContext context) { _context = context; }

        [BindProperty]
        public Recordatorio Recordatorio { get; set; } = new();
        public List<Cliente> Clientes { get; set; } = new();
        public List<Motivo_Recordatorio> MotivosRecordatorio { get; set; } = new();
        public string ClienteDescripcion { get; set; } = "";
        public string MotivoDescripcion { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            Clientes = await _context.Cliente.OrderBy(c => c.Nombre).ToListAsync();
            MotivosRecordatorio = await _context.Motivo_Recordatorio.OrderBy(m => m.Descripcion).ToListAsync();
            
            if (id.HasValue)
            {
                Recordatorio = await _context.Recordatorio
                    .Include(r => r.Cliente)
                    .Include(r => r.Motivo_Recordatorio)
                    .Include(r => r.RecordatorioPadre)
                    .FirstOrDefaultAsync(r => r.Id == id.Value);
                
                if (Recordatorio == null) return NotFound();
                
                ClienteDescripcion = $"{Recordatorio.Cliente?.Nombre} {Recordatorio.Cliente?.Primer_Apellido} {Recordatorio.Cliente?.Segundo_Apellido}".Trim();
                MotivoDescripcion = Recordatorio.Motivo_Recordatorio?.Descripcion ?? "";
            }
            else
            {
                // Crear fecha base sin segundos ni milisegundos
                var fechaBase = DateTime.Now.AddHours(1);
                var fechaSinSegundos = new DateTime(
                    fechaBase.Year, 
                    fechaBase.Month, 
                    fechaBase.Day, 
                    fechaBase.Hour, 
                    fechaBase.Minute, 
                    0, // segundos
                    0  // milisegundos
                );
                
                Recordatorio = new Recordatorio
                {
                    Fecha_Hora = fechaSinSegundos,
                    IntervaloRecurrencia = 1
                };
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Clientes = await _context.Cliente.OrderBy(c => c.Nombre).ToListAsync();
            MotivosRecordatorio = await _context.Motivo_Recordatorio.OrderBy(m => m.Descripcion).ToListAsync();
            
            ModelState.Remove("Recordatorio.Motivo_Recordatorio");
            ModelState.Remove("Recordatorio.Cliente");
            ModelState.Remove("Recordatorio.RecordatorioPadre");
            ModelState.Remove("Recordatorio.RecordatoriosHijo");

            // Validaciones de recurrencia
            if (Recordatorio.EsRecurrente)
            {
                if (!Recordatorio.TipoRecurrencia.HasValue)
                {
                    ModelState.AddModelError("Recordatorio.TipoRecurrencia", "Debe seleccionar un tipo de recurrencia");
                }

                if (Recordatorio.IntervaloRecurrencia < 1)
                {
                    ModelState.AddModelError("Recordatorio.IntervaloRecurrencia", "El intervalo debe ser mayor a 0");
                }

                if (Recordatorio.FechaFinRecurrencia.HasValue && Recordatorio.FechaFinRecurrencia <= Recordatorio.Fecha_Hora)
                {
                    ModelState.AddModelError("Recordatorio.FechaFinRecurrencia", "La fecha de fin debe ser posterior a la fecha del recordatorio");
                }

                if (Recordatorio.MaximoRepeticiones.HasValue && Recordatorio.MaximoRepeticiones < 1)
                {
                    ModelState.AddModelError("Recordatorio.MaximoRepeticiones", "El número de repeticiones debe ser mayor a 0");
                }

                if (!Recordatorio.FechaFinRecurrencia.HasValue && !Recordatorio.MaximoRepeticiones.HasValue)
                {
                    ModelState.AddModelError("", "Debe especificar una fecha de fin o un número máximo de repeticiones para la recurrencia");
                }
            }
            
            if (!ModelState.IsValid) return Page();
            
            if (Recordatorio.Id > 0)
            {
                var existente = await _context.Recordatorio.FindAsync(Recordatorio.Id);
                if (existente == null) return NotFound();
                
                existente.Fecha_Hora = Recordatorio.Fecha_Hora;
                existente.Detalles = Recordatorio.Detalles;
                existente.ClienteId = Recordatorio.ClienteId;
                existente.Motivo_RecordatorioId = Recordatorio.Motivo_RecordatorioId;
                
                // Solo actualizar recurrencia si no es un recordatorio hijo
                if (existente.RecordatorioPadreId == null)
                {
                    existente.EsRecurrente = Recordatorio.EsRecurrente;
                    existente.TipoRecurrencia = Recordatorio.TipoRecurrencia;
                    existente.IntervaloRecurrencia = Recordatorio.IntervaloRecurrencia;
                    existente.FechaFinRecurrencia = Recordatorio.FechaFinRecurrencia;
                    existente.MaximoRepeticiones = Recordatorio.MaximoRepeticiones;
                }
                
                _context.Recordatorio.Update(existente);
            }
            else
            {
                _context.Recordatorio.Add(Recordatorio);
                await _context.SaveChangesAsync();

                // Generar recordatorios recurrentes si es necesario
                if (Recordatorio.EsRecurrente)
                {
                    await GenerarRecordatoriosRecurrentes(Recordatorio);
                }
            }
            
            await _context.SaveChangesAsync();
            return RedirectToPage("/Recordatorios/Index");
        }

        private async Task GenerarRecordatoriosRecurrentes(Recordatorio recordatorioPadre)
        {
            var recordatoriosGenerados = new List<Recordatorio>();
            var fechaActual = recordatorioPadre.Fecha_Hora;
            var contador = 0;

            while (ShouldContinueGenerating(fechaActual, contador, recordatorioPadre))
            {
                fechaActual = CalcularSiguienteFecha(fechaActual, recordatorioPadre.TipoRecurrencia.Value, recordatorioPadre.IntervaloRecurrencia);
                contador++;

                // Verificar si debemos continuar
                if (!ShouldContinueGenerating(fechaActual, contador, recordatorioPadre))
                    break;

                var nuevoRecordatorio = new Recordatorio
                {
                    Fecha_Hora = fechaActual,
                    ClienteId = recordatorioPadre.ClienteId,
                    Motivo_RecordatorioId = recordatorioPadre.Motivo_RecordatorioId,
                    Detalles = recordatorioPadre.Detalles,
                    RecordatorioPadreId = recordatorioPadre.Id,
                    EsRecurrente = false // Los recordatorios hijo no son recurrentes por sí mismos
                };

                recordatoriosGenerados.Add(nuevoRecordatorio);

                // Limitar a 100 recordatorios por seguridad
                if (recordatoriosGenerados.Count >= 100)
                    break;
            }

            if (recordatoriosGenerados.Any())
            {
                _context.Recordatorio.AddRange(recordatoriosGenerados);
            }
        }

        private bool ShouldContinueGenerating(DateTime fecha, int contador, Recordatorio recordatorio)
        {
            // Verificar límite por número de repeticiones
            if (recordatorio.MaximoRepeticiones.HasValue && contador >= recordatorio.MaximoRepeticiones.Value)
                return false;

            // Verificar límite por fecha
            if (recordatorio.FechaFinRecurrencia.HasValue && fecha > recordatorio.FechaFinRecurrencia.Value)
                return false;

            // Verificar que no sea más de 2 años en el futuro
            if (fecha > DateTime.Now.AddYears(2))
                return false;

            return true;
        }

        private DateTime CalcularSiguienteFecha(DateTime fechaActual, TipoRecurrencia tipo, int intervalo)
        {
            return tipo switch
            {
                TipoRecurrencia.Diario => fechaActual.AddDays(intervalo),
                TipoRecurrencia.Semanal => fechaActual.AddDays(7 * intervalo),
                TipoRecurrencia.Mensual => fechaActual.AddMonths(intervalo),
                TipoRecurrencia.Anual => fechaActual.AddYears(intervalo),
                _ => fechaActual
            };
        }
    }
}