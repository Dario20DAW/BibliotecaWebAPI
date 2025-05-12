using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs
{
    public class RestriccionDominioCrearDTO
    {

        public int LlaveId {  get; set; }

        [Required]
        public required string Dominio { get; set; }
    }
}
