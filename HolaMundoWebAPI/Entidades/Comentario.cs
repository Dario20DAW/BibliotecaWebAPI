using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.Entidades
{
    public class Comentario
    {

        public Guid id { get; set; }
        [Required]
        public required string Cuerpo { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public int libroId { get; set; }
        public Libro? Libro { get; set; }

    }
}
