namespace ProyectoArqSoft.Domain.DTOs
{
    public class ComprobanteVentaPdfDto
    {
        public DateTime Fecha { get; set; }
        public string Nit { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string Cajero { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public List<ComprobanteVentaDetallePdfDto> Detalles { get; set; } = new();
    }
}