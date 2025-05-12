using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs
{
    public class RestriccionIPCrearDTO
    {

        public int LlaveId {  get; set; }

        [Required]
        public required string IP { get; set; }
    }
}
