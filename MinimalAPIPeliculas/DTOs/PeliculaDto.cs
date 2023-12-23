namespace MinimalAPIPeliculas.DTOs
{
    public class PeliculaDto
    {
        public int Id { get; set; }

        public string Titulo { get; set; } = null!;

        public bool EnCines { get; set; }

        public DateTime FechaLanzamiento { get; set; }

        public string? Poster { get; set; }

        public List<ComentarioDto> Comentarios { get; set; } = new();
        
        public List<GeneroDto> Generos { get; set; } = new();
        
        public List<ActorPeliculaDto> Actores { get; set; } = new();
    }
}