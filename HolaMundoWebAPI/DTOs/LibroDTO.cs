using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs
{
    public class LibroDTO
    {

        public int id { get; set; }

        [Required]
        public required string Titulo { get; set; }
    }
}
