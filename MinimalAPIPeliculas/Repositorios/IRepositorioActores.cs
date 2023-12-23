using MinimalAPIPeliculas.DTOs;
using MinimalAPIPeliculas.Entidades;

namespace MinimalAPIPeliculas.Repositorios
{
    public interface IRepositorioActores
    {
        Task<List<Actor>> ObtenerTodos(PaginacionDto paginacionDto);
        Task<List<Actor>> ObtenerPorNombre(string nombre);
        Task<Actor?> ObtenerPorId(int id);
        Task<int> Crear(Actor actor);
        Task<bool> Existe(int id);
        Task<List<int>> Existen(List<int> ids);
        Task Actualizar(Actor actor);
        Task Borrar(int id);
    }
}
