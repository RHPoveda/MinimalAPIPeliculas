using AutoMapper;
using MinimalAPIPeliculas.DTOs;
using MinimalAPIPeliculas.Entidades;

namespace MinimalAPIPeliculas.Utilidades
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<CrearGeneroDto, Genero>();
            CreateMap<ActualizarGeneroDto, Genero>();
            CreateMap<Genero, GeneroDto>();

            CreateMap<CrearActorDto, Actor>()
                .ForMember(x => x.Foto, options => options.Ignore());
            CreateMap<Actor, ActorDto>();

            CreateMap<CrearPeliculaDto, Pelicula>()
                .ForMember(x => x.Poster, options => options.Ignore());
            CreateMap<Pelicula, PeliculaDto>()
                .ForMember(p => p.Generos,
                    ent =>
                        ent.MapFrom(p => p.GenerosPeliculas.Select(gp => new GeneroDto { Id = gp.GeneroId, Nombre = gp.Genero.Nombre })))
                .ForMember(p => p.Actores, ent => ent
                    .MapFrom(p => p.ActoresPeliculas.Select(ap => new ActorPeliculaDto { Id = ap.ActorId, Nombre = ap.Actor.Nombre, Personaje = ap.Personaje })));

            CreateMap<CrearComentarioDto, Comentario>();
            CreateMap<Comentario, ComentarioDto>();

            CreateMap<AsignarActorPeliculaDto, ActorPelicula>();
        }
    }
}
