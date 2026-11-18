namespace Nextag.Api.Models.Dto.Notificaciones
{
    public class NotificacionListaDto
    {
        public Guid Id { get; set; }

        public Guid EmpresaId { get; set; }

        public Guid? TicketId { get; set; }

        public string EventoClave { get; set; }
            = string.Empty;

        public string Titulo { get; set; }
            = string.Empty;

        public string Mensaje { get; set; }
            = string.Empty;

        public string? RutaNavegacion { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaLectura { get; set; }

        public bool Leida => FechaLectura.HasValue;
    }

    // ==========================================
    // RESULTADO PAGINADO
    // ==========================================

    public class NotificacionPaginadoDto
    {
        public List<NotificacionListaDto> Items { get; set; }
            = new();

        public int TotalRegistros { get; set; }

        public int Pagina { get; set; }

        public int TamanoPagina { get; set; }

        public int TotalPaginas { get; set; }
    }

    // ==========================================
    // CONTADOR
    // ==========================================

    public class NotificacionContadorDto
    {
        public int NoLeidas { get; set; }
    }
}
