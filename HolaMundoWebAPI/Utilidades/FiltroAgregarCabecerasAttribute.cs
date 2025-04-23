using Microsoft.AspNetCore.Mvc.Filters;

namespace BibliotecaAPI.Utilidades
{
    public class FiltroAgregarCabecerasAttribute: ActionFilterAttribute
    {
        private readonly string nombre;
        private readonly string valor;

        public FiltroAgregarCabecerasAttribute(string nombre, string valor)
        {
            this.nombre = nombre;
            this.valor = valor;
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {

            //antes de ejecutar la accion
            context.HttpContext.Response.Headers.Append(nombre, valor);
            base.OnResultExecuting(context);
            //despues de ejecutar la accion
        }

    }
}
