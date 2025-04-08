using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BibliotecaAPI.Controllers
{


    [ApiController]
    [Route("api/usuarios")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<IdentityUser> signInManager;

        public UsuariosController(UserManager<IdentityUser> userManager, IConfiguration configuration, SignInManager<IdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
        }


        [HttpPost("registro")]
        [AllowAnonymous]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Registrar(
            CredencialesUsuarioDTO credencialesUsuarioDTO)
        {
            var Usuario = new IdentityUser
            {
                UserName = credencialesUsuarioDTO.Email,
                Email = credencialesUsuarioDTO.Email
            };
                
            var resultado = await userManager
                .CreateAsync(Usuario, credencialesUsuarioDTO.Password!);

            if(resultado.Succeeded)
            {
                var respuestaAutenticacion = await ConstruirToken(credencialesUsuarioDTO);
                return respuestaAutenticacion;

            }
            else
            {
                foreach(var error in resultado.Errors)
                {
                    ModelState.AddModelError(String.Empty, error.Description);
                }

                return ValidationProblem();
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Login(
            CredencialesUsuarioDTO credencialesUsuarioDTO)
        { 

            var usuario = await userManager.FindByEmailAsync(credencialesUsuarioDTO.Email);

            if(usuario is null)
            {
                return RetornarLoginIncorrecto();
            }

            var resultado = await signInManager.CheckPasswordSignInAsync(usuario, 
                credencialesUsuarioDTO.Password!, lockoutOnFailure: false);

            if (resultado.Succeeded) 
            {
                return await ConstruirToken(credencialesUsuarioDTO);
            }

            else
            {
                return RetornarLoginIncorrecto();
            }
        }

        private ActionResult RetornarLoginIncorrecto()
        {
            ModelState.AddModelError(String.Empty, "Login incorrecto");
            return ValidationProblem();
        }


        private async Task<RespuestaAutenticacionDTO> ConstruirToken(
            CredencialesUsuarioDTO credencialesUsuarioDTO)
        {
            var claims = new List<Claim>
            {
                new Claim("email", credencialesUsuarioDTO?.Email),
                new Claim("lo que yo quiera", "valor aaa")

            };

            var usuario = await userManager.FindByEmailAsync(credencialesUsuarioDTO.Email);
            var claimsDB = await userManager.GetClaimsAsync(usuario!);

            claims.AddRange(claimsDB);

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                configuration["llavejwt"]!));

            var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddYears(2);

            var tokeDeSeguridad = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiracion,
                signingCredentials: credenciales
                );

            var token = new JwtSecurityTokenHandler().WriteToken( tokeDeSeguridad );

            return new RespuestaAutenticacionDTO
            {
                Token = token, 
                Expiracion = expiracion,
            };
        }

    }
}
