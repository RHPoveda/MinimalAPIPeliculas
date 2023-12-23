using FluentValidation;
using MinimalAPIPeliculas.DTOs;

namespace MinimalAPIPeliculas.Validaciones
{
    public class CrearComentarioDtoValidacion : AbstractValidator<CrearComentarioDto>
    {
        public CrearComentarioDtoValidacion()
        {
            RuleFor(x => x.Cuerpo).NotEmpty().WithMessage(Utilidades.CampoRequeridoMensaje);
        }
    }
}
