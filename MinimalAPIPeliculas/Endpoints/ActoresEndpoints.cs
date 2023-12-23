using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.OpenApi.Models;
using MinimalAPIPeliculas.DTOs;
using MinimalAPIPeliculas.Entidades;
using MinimalAPIPeliculas.Filtros;
using MinimalAPIPeliculas.Repositorios;
using MinimalAPIPeliculas.Servicios;
using MinimalAPIPeliculas.Utilidades;

namespace MinimalAPIPeliculas.Endpoints
{
    public static class ActoresEndpoints
    {
        private static readonly string contenedor = "actores";

        public static RouteGroupBuilder MapActores(this RouteGroupBuilder group)
        {
            group.MapGet("/", ObtenerTodos).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("actores-get"))
                .AgregarParametrosPaginacionAOpenApi();

            group.MapGet("/{id:int}", ObtenerPorId);

            group.MapGet("obtenerPorNombre/{nombre}", ObtenerPorNombre);

            group.MapPost("/", Crear).DisableAntiforgery()
                .AddEndpointFilter<FiltroValidaciones<CrearActorDto>>()
                .RequireAuthorization("esadmin")
                .WithOpenApi();

            group.MapPut("/{id:int}", Actualizar).DisableAntiforgery()
                .AddEndpointFilter<FiltroValidaciones<CrearActorDto>>()
                .RequireAuthorization("esadmin")
                .WithOpenApi();

            group.MapDelete("/{id:int}", Borrar)
                .RequireAuthorization("esadmin");

            return group;
        }

        static async Task<Ok<List<ActorDto>>> ObtenerTodos(IRepositorioActores repo, IMapper mapper, PaginacionDto paginacion)
        {
            //var paginacion = new PaginacionDto
            //{
            //    Pagina = pagina,
            //    RecordsPorPagina = recordsPorPagina
            //};

            var actores = await repo.ObtenerTodos(paginacion);
            var actoresDto = mapper.Map<List<ActorDto>>(actores);


            return TypedResults.Ok(actoresDto);
        }

        static async Task<Ok<List<ActorDto>>> ObtenerPorNombre(string nombre, IRepositorioActores repo, IMapper mapper)
        {
            var actores = await repo.ObtenerPorNombre(nombre);
            var actoresDto = mapper.Map<List<ActorDto>>(actores);


            return TypedResults.Ok(actoresDto);
        }

        static async Task<Results<Ok<ActorDto>, NotFound>> ObtenerPorId(IRepositorioActores repo, int id, IMapper mapper)
        {
            var actor = await repo.ObtenerPorId(id);

            if (actor is null)
            {
                return TypedResults.NotFound();
            }

            var actorDto = mapper.Map<ActorDto>(actor);

            return TypedResults.Ok(actorDto);
        }

        static async Task<Results<Created<ActorDto>, ValidationProblem>> Crear([FromForm] CrearActorDto crearActorDto, IRepositorioActores repo, IOutputCacheStore outputCacheStore,
            IMapper mapper, IAlmacenadorArchivos almacenadorArchivos)
        {

            var actor = mapper.Map<Actor>(crearActorDto);

            if (crearActorDto.Foto is not null)
            {
                var url = await almacenadorArchivos.Almacenar(contenedor, crearActorDto.Foto);
                actor.Foto = url;
            }

            var id = await repo.Crear(actor);

            await outputCacheStore.EvictByTagAsync("actores-get", default);
            var actorDto = mapper.Map<ActorDto>(actor);
            return TypedResults.Created($"/actores/{id}", actorDto);
        }

        static async Task<Results<NoContent, NotFound>> Actualizar(int id, [FromForm] CrearActorDto crearActorDto, IRepositorioActores repo,
            IAlmacenadorArchivos almacenadorArchivos, IOutputCacheStore outputCacheStore, IMapper mapper)
        {
            var actorDb = await repo.ObtenerPorId(id);
            if (actorDb is null)
            {
                return TypedResults.NotFound();
            }

            var actorParaActualizar = mapper.Map<Actor>(crearActorDto);
            actorParaActualizar.Id = actorDb.Id;
            actorParaActualizar.Foto = actorDb.Foto;

            if (crearActorDto.Foto is not null)
            {
                var url = await almacenadorArchivos.Editar(actorParaActualizar.Foto, contenedor, crearActorDto.Foto);
                actorParaActualizar.Foto = url;
            }

            await repo.Actualizar(actorParaActualizar);
            await outputCacheStore.EvictByTagAsync("actores-get", default);
            return TypedResults.NoContent();

        }

        static async Task<Results<NoContent, NotFound>> Borrar(int id, IRepositorioActores repo, IOutputCacheStore outputCacheStore, IAlmacenadorArchivos almacenadorArchivos)
        {
            var actorDb = await repo.ObtenerPorId(id);
            if (actorDb is null)
            {
                return TypedResults.NotFound();
            }

            await repo.Borrar(id);
            await almacenadorArchivos.Borrar(actorDb.Foto, contenedor);
            await outputCacheStore.EvictByTagAsync("actores-get", default);

            return TypedResults.NoContent();
        }
    }
}
