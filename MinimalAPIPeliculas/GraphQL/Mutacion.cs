using AutoMapper;
using HotChocolate.Authorization;
using MinimalAPIPeliculas.DTOs;
using MinimalAPIPeliculas.Entidades;
using MinimalAPIPeliculas.Repositorios;

namespace MinimalAPIPeliculas.GraphQL
{
    public class Mutacion
    {
        [Serial]
        [Authorize(Policy = "esadmin")]
        public async Task<GeneroDto> InsertarGenero([Service] IRepositorioGeneros repositorioGeneros,
            [Service] IMapper mapper, CrearGeneroDto crearGeneroDto)
        {
            var genero = mapper.Map<Genero>(crearGeneroDto);
            await repositorioGeneros.Crear(genero);
            var generoDto = mapper.Map<GeneroDto>(genero);
            return generoDto;
        }

        [Serial]
        [Authorize(Policy = "esadmin")]
        public async Task<GeneroDto?> ActualizarGenero([Service] IRepositorioGeneros repositorioGeneros,
            [Service] IMapper mapper, ActualizarGeneroDto actualizarGeneroDto)
        {
            var generoExiste = await repositorioGeneros.Existe(actualizarGeneroDto.Id);

            if (!generoExiste)
            {
                return null;
            }

            var genero = mapper.Map<Genero>(actualizarGeneroDto);
            await repositorioGeneros.Actualizar(genero);
            var generoDto = mapper.Map<GeneroDto>(genero);
            return generoDto;
        }

        [Serial]
        [Authorize(Policy = "esadmin")]
        public async Task<bool> BorrarGenero([Service] IRepositorioGeneros repositorioGeneros,
            int id)
        {
            var generoExiste = await repositorioGeneros.Existe(id);

            if (!generoExiste)
            {
                return false;
            }

            await repositorioGeneros.Borrar(id);
            return true;
        }
    }
}
