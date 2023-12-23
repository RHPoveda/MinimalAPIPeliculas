using Microsoft.EntityFrameworkCore;
using MinimalAPIPeliculas.Entidades;

namespace MinimalAPIPeliculas.Repositorios
{
    public class RepositorioGeneros : IRepositorioGeneros
    {
        private readonly ApplicationDbContext _context;

        public RepositorioGeneros(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Existe(int id)
        {
            return await _context.Generos.AnyAsync(x => x.Id == id);
        }

        public async Task<bool> Existe(int id, string nombre)
        {
            return await _context.Generos.AnyAsync(x => (x.Id != id) && (x.Nombre == nombre));
        }

        public async Task<List<int>> Existen(List<int> ids)
        {
            return await _context.Generos.Where(g => ids.Contains(g.Id)).Select(g => g.Id).ToListAsync();
        }

        public async Task<Genero?> ObtenerPorId(int id)
        {
            return await _context.Generos.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Genero>> ObtenerTodos()
        {
            return await _context.Generos.OrderBy(x => x.Nombre).ToListAsync();
        }

        public async Task<int> Crear(Genero genero)
        {
            _context.Add(genero);
            await _context.SaveChangesAsync();
            return genero.Id;
        }

        public async Task Actualizar(Genero genero)
        {
            _context.Update(genero);
            await _context.SaveChangesAsync();
        }

        public async Task Borrar(int id)
        {
            await _context.Generos.Where(x => x.Id == id).ExecuteDeleteAsync();
        }
    }
}
