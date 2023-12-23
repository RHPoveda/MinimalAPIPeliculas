using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIPeliculas.DTOs;
using MinimalAPIPeliculas.Entidades;
using MinimalAPIPeliculas.Filtros;
using MinimalAPIPeliculas.Repositorios;
using MinimalAPIPeliculas.Servicios;
using MinimalAPIPeliculas.Utilidades;

namespace MinimalAPIPeliculas.Endpoints
{
    public static class PeliculasEndpoints
    {
        private static readonly string contenedor = "peliculas";

        public static RouteGroupBuilder MapPeliculas(this RouteGroupBuilder group)
        {
            group.MapGet("/", ObtenerTodos).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("peliculas-get"))
                .AgregarParametrosPaginacionAOpenApi();

            group.MapGet("/{id:int}", ObtenerPorId);
            
            group.MapPost("/", Crear).DisableAntiforgery().AddEndpointFilter<FiltroValidaciones<CrearPeliculaDto>>()
                .RequireAuthorization("esadmin")
                .WithOpenApi();

            group.MapPut("/{id:int}", Actualizar).DisableAntiforgery().AddEndpointFilter<FiltroValidaciones<CrearPeliculaDto>>()
                .RequireAuthorization("esadmin")
                .WithOpenApi();

            group.MapDelete("/{id:int}", Borrar)
                .RequireAuthorization("esadmin");

            group.MapPost("/{id:int}/asignarGeneros", AsignarGeneros)
                .RequireAuthorization("esadmin");

            group.MapPost("/{id:int}/asignarActores", AsignarActores)
                .RequireAuthorization("esadmin");

            group.MapGet("/filtrar", FiltrarPeliculas).AgregarParametrosPeliculasFiltroAOpenApi();

            return group;
        }

        static async Task<Ok<List<PeliculaDto>>> ObtenerTodos(IRepositorioPeliculas repo, IMapper mapper,
            PaginacionDto paginacion)
        {
            var peliculas = await repo.ObtenerTodos(paginacion);
            var peliculaDtos = mapper.Map<List<PeliculaDto>>(peliculas);


            return TypedResults.Ok(peliculaDtos);
        }

        static async Task<Results<Ok<PeliculaDto>, NotFound>> ObtenerPorId(IRepositorioPeliculas repo, int id,
            IMapper mapper)
        {
            var pelicula = await repo.ObtenerPorId(id);

            if (pelicula is null)
            {
                return TypedResults.NotFound();
            }

            var peliculaDto = mapper.Map<PeliculaDto>(pelicula);

            return TypedResults.Ok(peliculaDto);
        }

        static async Task<Created<PeliculaDto>> Crear([FromForm] CrearPeliculaDto crearPeliculaDto,
            IRepositorioPeliculas repo, IOutputCacheStore outputCacheStore, IMapper mapper,
            IAlmacenadorArchivos almacenadorArchivos)
        {
            var pelicula = mapper.Map<Pelicula>(crearPeliculaDto);

            if (crearPeliculaDto.Poster is not null)
            {
                var url = await almacenadorArchivos.Almacenar(contenedor, crearPeliculaDto.Poster);
                pelicula.Poster = url;
            }

            var id = await repo.Crear(pelicula);

            await outputCacheStore.EvictByTagAsync("peliculas-get", default);
            var peliculaDto = mapper.Map<PeliculaDto>(pelicula);
            return TypedResults.Created($"/peliculas/{id}", peliculaDto);
        }

        static async Task<Results<NoContent, NotFound>> Actualizar(int id, [FromForm] CrearPeliculaDto crearPeliculaDto,
            IRepositorioPeliculas repo,
            IAlmacenadorArchivos almacenadorArchivos, IOutputCacheStore outputCacheStore, IMapper mapper)
        {
            var peliculaDb = await repo.ObtenerPorId(id);
            if (peliculaDb is null)
            {
                return TypedResults.NotFound();
            }

            var peliculaParaActualizar = mapper.Map<Pelicula>(crearPeliculaDto);
            peliculaParaActualizar.Id = peliculaDb.Id;
            peliculaParaActualizar.Poster = peliculaDb.Poster;

            if (crearPeliculaDto.Poster is not null)
            {
                var url = await almacenadorArchivos.Editar(peliculaParaActualizar.Poster, contenedor,
                    crearPeliculaDto.Poster);
                peliculaParaActualizar.Poster = url;
            }

            await repo.Actualizar(peliculaParaActualizar);
            await outputCacheStore.EvictByTagAsync("peliculas-get", default);
            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound>> Borrar(int id, IRepositorioPeliculas repo, IOutputCacheStore outputCacheStore, IAlmacenadorArchivos almacenadorArchivos)
        {
            var peliculaDb = await repo.ObtenerPorId(id);
            if (peliculaDb is null)
            {
                return TypedResults.NotFound();
            }

            await repo.Borrar(id);
            await almacenadorArchivos.Borrar(peliculaDb.Poster, contenedor);
            await outputCacheStore.EvictByTagAsync("peliculas-get", default);

            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound, BadRequest<string>>> AsignarGeneros(int id, List<int> generosIds,
            IRepositorioPeliculas repoPeliculas, IRepositorioGeneros repoGeneros)
        {
            if (!await repoPeliculas.Existe(id))
            {
                return TypedResults.NotFound();
            }

            var generosExistentes = new List<int>();

            if (generosIds.Count != 0)
            {
                generosExistentes = await repoGeneros.Existen(generosIds);
            }

            if (generosExistentes.Count != generosIds.Count)
            {
                var generosNoExistentes = generosIds.Except(generosExistentes);
                return TypedResults.BadRequest($"Los géneros de id {string.Join(",", generosNoExistentes)} no existen");
            }

            await repoPeliculas.AsignarGeneros(id, generosIds);

            return TypedResults.NoContent();

        }

        static async Task<Results<NoContent, NotFound, BadRequest<string>>> AsignarActores(int id, List<AsignarActorPeliculaDto> actoresDto,
            IRepositorioPeliculas repoPeliculas, IRepositorioActores repoActores, IMapper mapper)
        {
            if (!await repoPeliculas.Existe(id))
            {
                return TypedResults.NotFound();
            }

            var actoresExistentes = new List<int>();
            var actoresIds = actoresDto.Select(a => a.ActorId).ToList();

            if (actoresDto.Count != 0)
            {
                actoresExistentes = await repoActores.Existen(actoresIds);
            }


            if (actoresExistentes.Count != actoresDto.Count)
            {
                var actoresNoExistentes = actoresIds.Except(actoresExistentes);
                return TypedResults.BadRequest($"Los actores de id {string.Join(",", actoresNoExistentes)} no existen");
            }

            var actores = mapper.Map<List<ActorPelicula>>(actoresDto);

            await repoPeliculas.AsignarActores(id, actores);
            return TypedResults.NoContent();

        }

        static async Task<Ok<List<PeliculaDto>>> FiltrarPeliculas(PeliculasFiltrarDto peliculasFiltrarDto,
            IRepositorioPeliculas repositorio, IMapper mapper)
        {
            var peliculas = await repositorio.Filtrar(peliculasFiltrarDto);
            var peliculasDto = mapper.Map<List<PeliculaDto>>(peliculas);
            return TypedResults.Ok(peliculasDto);
        }

    }
}
