using Error = MinimalAPIPeliculas.Entidades.Error;

namespace MinimalAPIPeliculas.Repositorios
{
    public class RepositorioErrores : IRepositorioErrores
    {
        private readonly ApplicationDbContext _context;

        public RepositorioErrores(ApplicationDbContext context)
        {
            this._context = context;
        }

        public async Task Crear(Error error)
        {
            _context.Add(error);
            await _context.SaveChangesAsync();
        }
    }
}
