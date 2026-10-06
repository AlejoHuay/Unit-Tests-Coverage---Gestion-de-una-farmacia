namespace ProyectoArqSoft.Domain.Models
{
    public class Venta
    {
        public int Id { get; set; }

        public DateTime FechaHora { get; set; }
        public decimal Total { get; set; }
        public string MetodoPago { get; set; } = string.Empty;

        public int IdCliente { get; set; }
        public int IdUsuario { get; set; }

        public short Estado { get; set; } = 1;
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimaActualizacion { get; set; }
        public int? IdUsuarioEditor { get; set; }
        public string Nit { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;

        public List<DetalleVenta> Detalles { get; set; } = new();
    }
}
