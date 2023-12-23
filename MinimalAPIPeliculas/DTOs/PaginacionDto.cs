using MinimalAPIPeliculas.Utilidades;

namespace MinimalAPIPeliculas.DTOs
{
    public class PaginacionDto
    {
        private const int paginaValorInicial = 1;
        private const int recordsPorPaginaValorInicial = 10;

        public int Pagina { get; set; } = 1;
        private int _recordsPorPagina = 10;
        private readonly int _cantidadMaximaRecordsPorPagina = 50;

        public int RecordsPorPagina
        {
            get => _recordsPorPagina;

            set => _recordsPorPagina = (value > _cantidadMaximaRecordsPorPagina) ? _cantidadMaximaRecordsPorPagina : value;
        }

        public static ValueTask<PaginacionDto> BindAsync(HttpContext context)
        {
            var pagina = context.ExtraerValorODefecto(nameof(Pagina), paginaValorInicial);
            var recordsPorPagina = context.ExtraerValorODefecto(nameof(RecordsPorPagina),
                recordsPorPaginaValorInicial);

            var resultado = new PaginacionDto
            {
                Pagina = pagina,
                RecordsPorPagina = recordsPorPagina
            };

            return ValueTask.FromResult(resultado);

        }
    }
}
