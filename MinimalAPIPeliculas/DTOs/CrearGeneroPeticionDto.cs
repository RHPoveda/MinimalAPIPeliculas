using AutoMapper;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIPeliculas.Repositorios;

namespace MinimalAPIPeliculas.DTOs
{
    public class CrearGeneroPeticionDto
    {
        public IRepositorioGeneros Repo { get; set; }
        public IOutputCacheStore OutputCacheStore { get; set; }
        public IMapper Mapper { get; set; }
    }
}
