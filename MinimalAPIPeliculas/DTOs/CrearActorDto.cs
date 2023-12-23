namespace MinimalAPIPeliculas.DTOs
{
    public class CrearActorDto
    {
        public string Nombre { get; set; } = null!;

        public DateTime FechaNacimiento { get; set; }

        public IFormFile? Foto { get; set; }
    }
}
