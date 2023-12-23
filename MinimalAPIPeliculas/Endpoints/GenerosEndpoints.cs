using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIPeliculas.DTOs;
using MinimalAPIPeliculas.Entidades;
using MinimalAPIPeliculas.Filtros;
using MinimalAPIPeliculas.Repositorios;

namespace MinimalAPIPeliculas.Endpoints
{
    public static class GenerosEndpoints
    {
        public static RouteGroupBuilder MapGeneros(this RouteGroupBuilder group)
        {
            group.MapGet("/", ObtenerGeneros)
                .CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("generos-get"));

            
            group.MapGet("/{id:int}", ObtenerGeneroPorId);

            
            //Crear genero
            group.MapPost("/", CrearGenero)
                .AddEndpointFilter<FiltroValidaciones<CrearGeneroDto>>()
                .RequireAuthorization("esadmin");
            
            
            // actualizar genero
            group.MapPut("/{id:int}", ActualizarGenero)
                .AddEndpointFilter<FiltroValidaciones<CrearGeneroDto>>()
                .RequireAuthorization("esadmin")
                .WithOpenApi(options =>
                {
                    options.Summary = "Actualizar un género";
                    options.Description = "Con este endpoint podemos actualizar un género";
                    options.Parameters[0].Description = "El id del género a actualizar";
                    options.RequestBody.Description = "El género que se desea actualizar";

                    return options;
                });


            // borrar gnero
            group.MapDelete("/{id:int}", BorrarGenero)
                .RequireAuthorization("esadmin");

            return group;
        }

        static async Task<Ok<List<GeneroDto>>> ObtenerGeneros(IRepositorioGeneros repo, IMapper mapper, ILoggerFactory loggerFactory)
        {
            var tipo = typeof(GenerosEndpoints);
            var logger = loggerFactory.CreateLogger(tipo.FullName!);
            logger.LogInformation("Obteniendo el listado de generos");


            var generos = await repo.ObtenerTodos();
            var generosDto = mapper.Map<List<GeneroDto>>(generos);


            return TypedResults.Ok(generosDto);
        }

        static async Task<Results<Ok<GeneroDto>, NotFound>> ObtenerGeneroPorId([AsParameters] ObtenerGeneroPorIdPeticionDto modelo)
        {
            var genero = await modelo.Repo.ObtenerPorId(modelo.Id);

            if (genero is null)
            {
                return TypedResults.NotFound();
            }

            var generoDto = modelo.Mapper.Map<GeneroDto>(genero);

            return TypedResults.Ok(generoDto);
        }

        static async Task<Results<Created<GeneroDto>, ValidationProblem>> CrearGenero(CrearGeneroDto crearGeneroDto,
            [AsParameters] CrearGeneroPeticionDto modelo)
        {

            var genero = modelo.Mapper.Map<Genero>(crearGeneroDto);

            var id = await modelo.Repo.Crear(genero);

            // Limpiar cache
            await modelo.OutputCacheStore.EvictByTagAsync("generos-get", default);

            var generoDto = modelo.Mapper.Map<GeneroDto>(genero);
            return TypedResults.Created($"/generos/{id}", generoDto);
        }

        static async Task<Results<NoContent, NotFound, ValidationProblem>> ActualizarGenero(int id, CrearGeneroDto crearGeneroDto, IRepositorioGeneros repo, 
            IOutputCacheStore outputCacheStore, IMapper mapper)
        {

            var existe = await repo.Existe(id);
            if (!existe)
            {
                return TypedResults.NotFound();
            }

            var genero = mapper.Map<Genero>(crearGeneroDto);
            genero.Id = id;

            await repo.Actualizar(genero);

            // Limpiar cache
            await outputCacheStore.EvictByTagAsync("generos-get", default);

            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound>> BorrarGenero(int id, IRepositorioGeneros repo, IOutputCacheStore outputCacheStore)
        {
            var existe = await repo.Existe(id);
            if (!existe)
            {
                return TypedResults.NotFound();
            }

            await repo.Borrar(id);

            // Limpiar cache
            await outputCacheStore.EvictByTagAsync("generos-get", default);

            return TypedResults.NoContent();
        }
    }
}
