using ProyectoArqSoft.Domain.DTOs;
using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;
using System.Data;

namespace ProyectoArqSoft.Application.Interfaces
{
    public interface IVentaService
    {
        DataTable ObtenerTodos();
        DataTable ObtenerTodos(string filtro);

        Venta? ObtenerPorId(int id);
        List<DetalleVenta> ObtenerDetallesPorVenta(int idVenta);

        Result Crear(
            int idCliente,
            int idUsuario,
            string metodoPago,
            List<DetalleVentaInputDto> detallesInput);

        Result Actualizar(
            int idVenta,
            int idCliente,
            string metodoPago,
            List<DetalleVentaInputDto> detallesInput,
            int idUsuarioEditor);

        Result EliminarLogicamente(int idVenta, int idUsuarioEditor);
    }
}
