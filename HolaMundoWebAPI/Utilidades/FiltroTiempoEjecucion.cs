using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BibliotecaAPI.Utilidades
{
    public class FiltroTiempoEjecucion : IAsyncActionFilter
    {
        private readonly ILogger<FiltroTiempoEjecucion> _logger;

        public FiltroTiempoEjecucion(ILogger<FiltroTiempoEjecucion> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //antes de ejecutar la accion
            var stopWatch = Stopwatch.StartNew();
            _logger.LogInformation($"INICIO Accion: {context.ActionDescriptor.DisplayName}");

            await next();

            //despues de ejecutar la accion
            stopWatch.Stop();
            _logger.LogInformation($"FIN Accion: {context.ActionDescriptor.DisplayName} - Tiempo: {stopWatch.ElapsedMilliseconds} ms");

        }
    }
}
