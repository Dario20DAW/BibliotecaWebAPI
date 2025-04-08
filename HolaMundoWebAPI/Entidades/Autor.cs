using System.ComponentModel.DataAnnotations;
using BibliotecaAPI.Validaciones;

namespace BibliotecaAPI.Entidades
{
    public class Autor
    {

        public int Id { get; set; }

        [Required(ErrorMessage ="EL campo {0} es requerido")]
        [StringLength(150, ErrorMessage = "El campo {0} debe tener {1} caracteres o menos")]
        [PrimeraLetraMayuscula]
        public required string Nombres { get; set; }


        [Required(ErrorMessage = "EL campo {0} es requerido")]
        [StringLength(150, ErrorMessage = "El campo {0} debe tener {1} caracteres o menos")]
        [PrimeraLetraMayuscula]
        public required string Apellidos { get; set; }

        [StringLength(20, ErrorMessage = "El campo {0} debe tener {1} caracteres o menos")]
        public String? Identificacion { get; set; }
        public List<AutorLibro> Libros { get; set; } = [];

    }
}
