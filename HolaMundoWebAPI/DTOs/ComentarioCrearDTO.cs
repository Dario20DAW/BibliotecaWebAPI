using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs
{
    public class ComentarioCrearDTO
    {

        [Required]
        public required string Cuerpo { get; set; }

    }
}
