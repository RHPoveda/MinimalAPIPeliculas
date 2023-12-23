using MinimalAPIPeliculas.DTOs;
using MinimalAPIPeliculas.Entidades;

namespace MinimalAPIPeliculas.Repositorios
{
    public interface IRepositorioPeliculas
    {
        Task<List<Pelicula>> ObtenerTodos(PaginacionDto paginacionDto);
        Task<Pelicula?> ObtenerPorId(int id);
        Task<int> Crear(Pelicula pelicula);
        Task<bool> Existe(int id);
        Task Actualizar(Pelicula pelicula);
        Task Borrar(int id);
        Task AsignarGeneros(int id, List<int> generosIds);
        Task AsignarActores(int id, List<ActorPelicula> actores);
        Task<List<Pelicula>> Filtrar(PeliculasFiltrarDto peliculasFiltrarDto);
    }
}
