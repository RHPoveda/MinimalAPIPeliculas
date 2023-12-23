using MinimalAPIPeliculas.DTOs;
using MinimalAPIPeliculas.Entidades;

namespace MinimalAPIPeliculas.Repositorios
{
    public interface IRepositorioComentarios
    {
        Task<List<Comentario>> ObtenerTodos(int peliculaId);
        Task<Comentario?> ObtenerPorId(int id);
        Task<int> Crear(Comentario comentario);
        Task<bool> Existe(int id);
        Task Actualizar(Comentario comentario);
        Task Borrar(int id);
    }
}
