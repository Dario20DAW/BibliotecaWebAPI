using Microsoft.AspNetCore.Mvc.Filters;

namespace BibliotecaAPI.Utilidades
{
    public class MiFiltroDeAccion : IActionFilter
    {
        private readonly ILogger<MiFiltroDeAccion> _logger;

        public MiFiltroDeAccion(ILogger<MiFiltroDeAccion> logger)
        {
            _logger = logger;
        }


        //antes de la accion
        public void OnActionExecuting(ActionExecutingContext context)
        {
            _logger.LogInformation("Ejecutando la accion");
        }




        //despues de la accion
        public void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.LogInformation("Accion Ejecutada");
        }


    }
}
