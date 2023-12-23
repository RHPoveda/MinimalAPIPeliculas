using FluentValidation;
using MinimalAPIPeliculas.DTOs;

namespace MinimalAPIPeliculas.Validaciones
{
    public class CrearActorDtoValidador : AbstractValidator<CrearActorDto>
    {
        public CrearActorDtoValidador()
        {
            RuleFor(x => x.Nombre).NotEmpty().WithMessage(Utilidades.CampoRequeridoMensaje)
                .MaximumLength(150).WithMessage(Utilidades.MaximumLengthMensaje);

            var fechaMinima = new DateTime(1900, 1, 1);

            RuleFor(x => x.FechaNacimiento).GreaterThanOrEqualTo(fechaMinima)
                .WithMessage(Utilidades.GreaterThanOrEqualToMensaje(fechaMinima));
        }
    }
}
