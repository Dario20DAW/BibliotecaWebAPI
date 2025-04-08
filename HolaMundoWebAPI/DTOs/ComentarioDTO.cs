using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs
{
    public class ComentarioDTO
    {

        public Guid id { get; set; }
        [Required]
        public required string Cuerpo { get; set; }
        public DateTime FechaPublicacion { get; set; }

    }
}
