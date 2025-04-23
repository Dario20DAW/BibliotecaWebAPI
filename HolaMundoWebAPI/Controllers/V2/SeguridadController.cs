using BibliotecaAPI.Servicios;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers.V2
{

    [Route("api/v2/seguridad")]
    [ApiController]
    public class SeguridadController : ControllerBase
    {
        private readonly IDataProtector protector;
        private readonly ITimeLimitedDataProtector protectorLimitado;
        private readonly IServicioHash servicioHash;

        public SeguridadController(IDataProtectionProvider protectionProvider, IServicioHash servicioHash)
        {
            protector = protectionProvider.CreateProtector("");
            protectorLimitado = protector.ToTimeLimitedDataProtector();
            this.servicioHash = servicioHash;
        }


        [HttpGet("hash")]
        public ActionResult Hash(string textoPlano) 
        {
            var hash1 = servicioHash.Hash(textoPlano);
            var hash2 = servicioHash.Hash(textoPlano);
            var hash3 = servicioHash.Hash(textoPlano, hash2.Sal);

            var resultado = new {textoPlano, hash1, hash2, hash3};

            return Ok(resultado);

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
