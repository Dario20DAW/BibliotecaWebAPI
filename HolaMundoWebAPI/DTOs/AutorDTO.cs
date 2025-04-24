namespace BibliotecaAPI.DTOs
{
    public class AutorDTO: RecursoDTO
    {

        public int Id { get; set; }

        public required String NombreCompleto { get; set; }
        public string? Foto { get; set; }

    }
}
