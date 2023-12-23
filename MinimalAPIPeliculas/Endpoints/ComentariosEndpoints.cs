using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIPeliculas.DTOs;
using MinimalAPIPeliculas.Entidades;
using MinimalAPIPeliculas.Filtros;
using MinimalAPIPeliculas.Repositorios;
using MinimalAPIPeliculas.Servicios;

namespace MinimalAPIPeliculas.Endpoints
{
    public static class ComentariosEndpoints
    {
        public static RouteGroupBuilder MapComentarios(this RouteGroupBuilder group)
        {
            group.MapGet("/", ObtenerTodos).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60))
                .Tag("comentarios-get")
                .SetVaryByRouteValue(new string[] { "peliculaId" }));

            group.MapGet("/{id:int}", ObtenerPorId);

            group.MapPost("/", Crear).AddEndpointFilter<FiltroValidaciones<CrearComentarioDto>>()
                .RequireAuthorization();

            group.MapPut("/{id:int}", Actualizar)
                .AddEndpointFilter<FiltroValidaciones<CrearComentarioDto>>()
                .RequireAuthorization();

            group.MapDelete("/{id:int}", Borrar)
                .RequireAuthorization();

            return group;
        }

        static async Task<Results<Ok<List<ComentarioDto>>, NotFound>> ObtenerTodos(int peliculaId,
            IRepositorioComentarios repoComentarios,
            IRepositorioPeliculas repoPeliculas,
            IMapper mapper)
        {
            if (!await repoPeliculas.Existe(peliculaId))
            {
                return TypedResults.NotFound();
            }

            var comentarios = await repoComentarios.ObtenerTodos(peliculaId);
            var comentariosDto = mapper.Map<List<ComentarioDto>>(comentarios);

            return TypedResults.Ok(comentariosDto);
        }

        static async Task<Results<Ok<ComentarioDto>, NotFound>> ObtenerPorId(IRepositorioComentarios repo, int peliculaId, int id,
            IMapper mapper)
        {
            var comentario = await repo
                .ObtenerPorId(id);

            if (comentario is null)
            {
                return TypedResults.NotFound();
            }

            var comentarioDto = mapper.Map<ComentarioDto>(comentario);

            return TypedResults.Ok(comentarioDto);
        }

        static async Task<Results<Created<ComentarioDto>, NotFound, BadRequest<string>>> Crear(int peliculaId,
            CrearComentarioDto crearComentarioDto, IRepositorioComentarios repoComentarios,
            IRepositorioPeliculas repoPeliculas,
            IMapper mapper, IOutputCacheStore outputCacheStore, IServicioUsuarios srvUsuarios)
        {
            if (!await repoPeliculas.Existe(peliculaId))
            {
                return TypedResults.NotFound();
            }

            var comentario = mapper.Map<Comentario>(crearComentarioDto);
            comentario.PeliculaId = peliculaId;


            var usuario = await srvUsuarios.ObtenerUsuario();
            if (usuario is null)
            {
                return TypedResults.BadRequest("Usuario no encontrado");
            }

            comentario.UsuarioId = usuario.Id;

            var id = await repoComentarios.Crear(comentario);

            await outputCacheStore.EvictByTagAsync("comentarios-get", default);
            var comentarioDto = mapper.Map<ComentarioDto>(comentario);

            return TypedResults.Created($"/comentario/{id}", comentarioDto);

        }

        static async Task<Results<NoContent, NotFound, ForbidHttpResult>> Actualizar(
            int peliculaId,
            int id,
            CrearComentarioDto crearComentarioDto,
            IOutputCacheStore outputCacheStore,
            IRepositorioComentarios repoComentarios,
            IRepositorioPeliculas repoPeliculas, IServicioUsuarios srvUsuarios
        )
        {
            if (!await repoPeliculas.Existe(peliculaId))
            {
                return TypedResults.NotFound();
            }

            var comentarioDb = await repoComentarios.ObtenerPorId(id);
            if (comentarioDb is null)
            {
                return TypedResults.NotFound();
            }

            var usuario = await srvUsuarios.ObtenerUsuario();
            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            if (comentarioDb.UsuarioId != usuario.Id)
            {
                return TypedResults.Forbid();
            }

            comentarioDb.Cuerpo = crearComentarioDto.Cuerpo;

            await repoComentarios.Actualizar(comentarioDb);
            await outputCacheStore.EvictByTagAsync("comentarios-get", default);
            return TypedResults.NoContent();

        }

        static async Task<Results<NoContent, NotFound, ForbidHttpResult>> Borrar(
            int peliculaId, int id, IRepositorioComentarios repoComentarios, IOutputCacheStore outputCacheStore, IServicioUsuarios srvUsuarios)
        {
            var comentarioDb = await repoComentarios.ObtenerPorId(id);
            if (comentarioDb is null)
            {
                return TypedResults.NotFound();
            }

            var usuario = await srvUsuarios.ObtenerUsuario();
            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            if (comentarioDb.UsuarioId != usuario.Id)
            {
                return TypedResults.Forbid();
            }

            await repoComentarios.Borrar(id);
            await outputCacheStore.EvictByTagAsync("comentarios-get", default);
            return TypedResults.NoContent();
        }
    }
}
