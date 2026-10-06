namespace ProyectoArqSoft.Domain.Models
{
    public class DetalleVenta
    {
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int IdVenta { get; set; }
        public int IdMedicamento { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}
