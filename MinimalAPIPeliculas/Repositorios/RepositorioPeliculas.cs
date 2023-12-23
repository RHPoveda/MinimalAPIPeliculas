using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MinimalAPIPeliculas.DTOs;
using MinimalAPIPeliculas.Entidades;
using MinimalAPIPeliculas.Utilidades;
using System.Linq.Dynamic.Core;

namespace MinimalAPIPeliculas.Repositorios
{
    public class RepositorioPeliculas : IRepositorioPeliculas
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly HttpContext _httpContextAccesor;
        private readonly ILogger<RepositorioPeliculas> _logger;

        public RepositorioPeliculas(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IMapper mapper, ILogger<RepositorioPeliculas> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _httpContextAccesor = httpContextAccessor.HttpContext!;
        }

        public async Task<List<Pelicula>> ObtenerTodos(PaginacionDto paginacion)
        {
            var queryable = _context.Peliculas.AsQueryable();
            await _httpContextAccesor.InsertarParametrosPaginacionEnCabecera(queryable);
            return await queryable.OrderBy(p => p.Titulo).Paginar(paginacion).ToListAsync();
        }

        public async Task<Pelicula?> ObtenerPorId(int id)
        {
            return await _context.Peliculas
                .Include(p => p.Comentarios)
                .Include(p => p.GenerosPeliculas)
                    .ThenInclude(gp => gp.Genero)
                .Include(p => p.ActoresPeliculas.OrderBy(a => a.Orden))
                    .ThenInclude(ap => ap.Actor)
                .AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<int> Crear(Pelicula pelicula)
        {
            _context.Add(pelicula);
            await _context.SaveChangesAsync();
            return pelicula.Id;
        }

        public async Task Actualizar(Pelicula pelicula)
        {
            _context.Update(pelicula);
            await _context.SaveChangesAsync();
        }

        public async Task Borrar(int id)
        {
            await _context.Peliculas.Where(x => x.Id == id).ExecuteDeleteAsync();
        }

        public async Task<bool> Existe(int id)
        {
            return await _context.Peliculas.AnyAsync(x => x.Id == id);
        }

        public async Task AsignarGeneros(int id, List<int> generosIds)
        {
            var pelicula = await _context.Peliculas
                .Include(p => p.GenerosPeliculas)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pelicula is null)
            {
                throw new ArgumentException($"No existe una pelicula con el id {id}");
            }

            var generosPeliculas = generosIds.Select(generoId => new GeneroPelicula()
            {
                GeneroId = generoId
            });

            pelicula.GenerosPeliculas = _mapper.Map(generosPeliculas, pelicula.GenerosPeliculas);
            await _context.SaveChangesAsync();

        }

        public async Task AsignarActores(int id, List<ActorPelicula> actores)
        {
            for (int i = 1; i <= actores.Count; i++)
            {
                actores[i - 1].Orden = i;
            }

            var pelicula = await _context.Peliculas
                .Include(x => x.ActoresPeliculas)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pelicula is null)
            {
                throw new ArgumentException($"No existe la pelicula con id: {id}");
            }

            pelicula.ActoresPeliculas = _mapper.Map(actores, pelicula.ActoresPeliculas);

            await _context.SaveChangesAsync();

        }

        public async Task<List<Pelicula>> Filtrar(PeliculasFiltrarDto peliculasFiltrarDto)
        {
            var peliculasQueryable = _context.Peliculas.AsQueryable();

            if (!string.IsNullOrWhiteSpace(peliculasFiltrarDto.Titulo))
            {
                peliculasQueryable = peliculasQueryable
                    .Where(p => p.Titulo.Contains(peliculasFiltrarDto.Titulo));
            }

            if (peliculasFiltrarDto.EnCines)
            {
                peliculasQueryable = peliculasQueryable.Where(p => p.EnCines);
            }

            if (peliculasFiltrarDto.ProximosEstrenos)
            {
                var hoy = DateTime.Today;
                peliculasQueryable = peliculasQueryable.Where(p => p.FechaLanzamiento > hoy);
            }

            if (peliculasFiltrarDto.GeneroId != 0)
            {
                peliculasQueryable = peliculasQueryable
                    .Where(p => p.GenerosPeliculas.Select(gp => gp.GeneroId)
                        .Contains(peliculasFiltrarDto.GeneroId));
            }

            if (!string.IsNullOrWhiteSpace(peliculasFiltrarDto.CampoOrdenar))
            {
                var tipoOrden = peliculasFiltrarDto.OrdenAscendente ? "ascending" : "descending";

                try
                {
                    // titulo ascending
                    peliculasQueryable = peliculasQueryable
                        .OrderBy($"{peliculasFiltrarDto.CampoOrdenar} {tipoOrden}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                }
            }

            await _httpContextAccesor.InsertarParametrosPaginacionEnCabecera(peliculasQueryable);

            var peliculas = await peliculasQueryable.Paginar(peliculasFiltrarDto.PaginacionDto).ToListAsync();

            return peliculas;
        }
    }
}
