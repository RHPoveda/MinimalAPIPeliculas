namespace MinimalAPIPeliculas.DTOs
{
    public class RespuestaAutenticacionDto
    {
        public string Token { get; set; } = null!;
        public DateTime Expiracion { get; set; }
    }
}
