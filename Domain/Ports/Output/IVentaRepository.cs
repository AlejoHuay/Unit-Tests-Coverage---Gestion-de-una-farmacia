using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;
using System.Data;

namespace ProyectoArqSoft.Application.Ports.Output
{
    public interface IVentaRepository
    {
        DataTable GetAll();
        DataTable GetAll(string filtro);

        Venta? GetById(int id);
        int Count();

        List<DetalleVenta> GetDetallesByVentaId(int idVenta);

        Result RegistrarVenta(Venta venta);
        Result ActualizarVenta(Venta venta);
        Result AnularVentaLogicamente(int idVenta, int idUsuarioEditor);
    }
}
