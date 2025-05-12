using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs
{
    public class RestriccionDominioActualizarDTO
    {


        [Required]
        public required string Dominio { get; set; }
    }
}
