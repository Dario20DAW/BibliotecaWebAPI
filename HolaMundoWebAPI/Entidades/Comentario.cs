using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

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
        public required string UsuarioId { get; set; }
        public bool EstaBorrado { get; set; }
        public Usuario? Usuario { get; set; }

    }
}
