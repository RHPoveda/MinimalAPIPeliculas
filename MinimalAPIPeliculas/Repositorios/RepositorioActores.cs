using Microsoft.EntityFrameworkCore;
using MinimalAPIPeliculas.DTOs;
using MinimalAPIPeliculas.Entidades;
using MinimalAPIPeliculas.Utilidades;

namespace MinimalAPIPeliculas.Repositorios
{
    public class RepositorioActores: IRepositorioActores
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpContext _httpContextAccessor;

        public RepositorioActores(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor.HttpContext!;
        }

        public async Task<bool> Existe(int id)
        {
            return await _context.Actores.AnyAsync(x => x.Id == id);
        }

        public async Task<List<int>> Existen(List<int> ids)
        {
            return await _context.Actores.Where(g => ids.Contains(g.Id)).Select(g => g.Id).ToListAsync();
        }

        public async Task<Actor?> ObtenerPorId(int id)
        {
            return await _context.Actores.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Actor>> ObtenerPorNombre(string nombre)
        {
            return await _context.Actores.Where(x => x.Nombre.Contains(nombre)).OrderBy(x => x.Nombre).ToListAsync();
        }

        public async Task<List<Actor>> ObtenerTodos(PaginacionDto paginacionDto)
        {
            var queryable = _context.Actores.AsQueryable();
            await _httpContextAccessor.InsertarParametrosPaginacionEnCabecera(queryable);
            return await queryable.OrderBy(x => x.Nombre).Paginar(paginacionDto).ToListAsync();
        }

        public async Task<int> Crear(Actor actor)
        {
            _context.Add(actor);
            await _context.SaveChangesAsync();
            return actor.Id;
        }

        public async Task Actualizar(Actor actor)
        {
            _context.Update(actor);
            await _context.SaveChangesAsync();
        }

        public async Task Borrar(int id)
        {
            await _context.Actores.Where(x => x.Id == id).ExecuteDeleteAsync();
        }
    }
}
