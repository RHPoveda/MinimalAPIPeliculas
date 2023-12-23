using FluentValidation;
using MinimalAPIPeliculas.DTOs;
using MinimalAPIPeliculas.Repositorios;

namespace MinimalAPIPeliculas.Validaciones
{
    public class CrearGeneroDtoValidador : AbstractValidator<CrearGeneroDto>
    {
        public CrearGeneroDtoValidador(IRepositorioGeneros repoGeneros, IHttpContextAccessor httpContextAccessor)
        {
            var valorDeRutaId = httpContextAccessor.HttpContext?.Request.RouteValues["id"];
            var id = 0;

            if (valorDeRutaId is string valorString)
            {
                int.TryParse(valorString, out id );

            }

            RuleFor(x => x.Nombre).NotEmpty().WithMessage(Utilidades.CampoRequeridoMensaje)
                .MaximumLength(50).WithMessage(Utilidades.MaximumLengthMensaje)
                .Must(PrimeraLetraEnMayuscula).WithMessage(Utilidades.PrimeraLetraMayusculaMensaje)
                .MustAsync(async (nombre, _) =>
                {
                    var existe = await repoGeneros.Existe(id, nombre);

                    return !existe;
                }).WithMessage(g => $"Ya existe un género con el nombre {g.Nombre}");
        }

        private bool PrimeraLetraEnMayuscula(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return true;
            }

            var primeraLetra = valor[0].ToString();

            return primeraLetra == primeraLetra.ToUpper();
        }
    }
}
