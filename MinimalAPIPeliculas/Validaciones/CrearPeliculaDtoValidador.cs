using FluentValidation;
using MinimalAPIPeliculas.DTOs;

namespace MinimalAPIPeliculas.Validaciones
{

    public class CrearPeliculaDtoValidador : AbstractValidator<CrearPeliculaDto>
    {
        public CrearPeliculaDtoValidador()
        {
            RuleFor(x => x.Titulo).NotEmpty().WithMessage(Utilidades.CampoRequeridoMensaje)
                .MaximumLength(150).WithMessage(Utilidades.MaximumLengthMensaje);
        }
    }

}
