using AutoMapper;
using MinimalAPIPeliculas.Repositorios;

namespace MinimalAPIPeliculas.DTOs
{
    public class ObtenerGeneroPorIdPeticionDto
    {
        public int Id { get; set; }
        public IRepositorioGeneros Repo { get; set; }
        public IMapper Mapper { get; set; }
    }
}
