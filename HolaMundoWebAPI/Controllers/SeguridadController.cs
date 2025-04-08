using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers
{

    [Route("api/seguridad")]
    [ApiController]
    public class SeguridadController : ControllerBase
    {
        private readonly IDataProtector protector;
        private readonly ITimeLimitedDataProtector protectorLimitado;

        public SeguridadController(IDataProtectionProvider protectionProvider)
        {
            protector = protectionProvider.CreateProtector("");
            protectorLimitado = protector.ToTimeLimitedDataProtector();
        }


        [HttpGet("encriptar")]
        public ActionResult Encriptar(string textoPlano)
        {
            string textoCifrado = protector.Protect(textoPlano);
            return Ok(new { textoCifrado });
        }


        [HttpGet("desencriptar")]
        public ActionResult Desencriptar(string textoCifrado)
        {
            string textoPlano = protector.Unprotect(textoCifrado);
            return Ok(new { textoPlano });
        }

        [HttpGet("encriptar-limitadoPorTiempo")]
        public ActionResult EncriptarLimitadoPorTiempo(string textoPlano)
        {
            string textoCifrado = protectorLimitado.Protect(textoPlano,
                lifetime: TimeSpan.FromSeconds(30));
            return Ok(new { textoCifrado });
        }


        [HttpGet("desencriptar-limitadoPorTiempo")]
        public ActionResult DesencriptarLimitadoPorTiempo(string textoCifrado)
        {
            string textoPlano = protectorLimitado.Unprotect(textoCifrado);
            return Ok(new { textoPlano });
        }
    }
}
