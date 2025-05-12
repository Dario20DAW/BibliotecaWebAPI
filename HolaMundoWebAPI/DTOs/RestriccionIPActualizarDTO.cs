using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs
{
    public class RestriccionIPActualizarDTO
    {


        [Required]
        public required string IP { get; set; }
    }
}
