using MinimalAPIPeliculas.Utilidades;

namespace MinimalAPIPeliculas.DTOs
{
    public class PeliculasFiltrarDto
    {
        public int Pagina { get; set; }

        public int RecordsPorPagina { get; set; }

        public PaginacionDto PaginacionDto =>
            new()
            {
                Pagina = Pagina,
                RecordsPorPagina = RecordsPorPagina
            };

        public string? Titulo { get; set; }

        public int GeneroId { get; set; }

        public bool EnCines { get; set; }

        public bool ProximosEstrenos { get; set; }

        public string? CampoOrdenar { get; set; }

        public bool OrdenAscendente { get; set; } = true;

        public static ValueTask<PeliculasFiltrarDto> BindAsync(HttpContext context)
        {
            var pagina = context.ExtraerValorODefecto(nameof(Pagina), 1);
            var recordsPorPagina = context.ExtraerValorODefecto(nameof(RecordsPorPagina), 10);
            var generoId = context.ExtraerValorODefecto(nameof(GeneroId), 0);

            var titulo = context.ExtraerValorODefecto(nameof(Titulo), string.Empty);
            var enCines = context.ExtraerValorODefecto(nameof(EnCines), false);
            var proximosEstrenos = context.ExtraerValorODefecto(nameof(ProximosEstrenos), false);
            var campoOrdenar = context.ExtraerValorODefecto(nameof(CampoOrdenar), string.Empty);
            var ordenAscendente = context.ExtraerValorODefecto(nameof(OrdenAscendente), true);

            var resultado = new PeliculasFiltrarDto
            {
                Pagina = pagina,
                RecordsPorPagina = recordsPorPagina,
                Titulo = titulo,
                GeneroId = generoId,
                EnCines = enCines,
                ProximosEstrenos = proximosEstrenos,
                CampoOrdenar = campoOrdenar,
                OrdenAscendente = ordenAscendente
            };

            return ValueTask.FromResult(resultado);
        }
    }
}
