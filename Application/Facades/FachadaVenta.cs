using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Domain.DTOs;
using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;
using System.Data;

namespace ProyectoArqSoft.Application.Facades
{
    public class FachadaVenta
    {
        private readonly IVentaService _ventaService;

        public FachadaVenta(IVentaService ventaService)
        {
            _ventaService = ventaService;
        }

        public Result RegistrarVenta(
            int idCliente,
            int idUsuario,
            string metodoPago,
            List<DetalleVentaInputDto> detalles)
        {
            return _ventaService.Crear(
                idCliente,
                idUsuario,
                metodoPago,
                detalles);
        }

        public Result ActualizarVenta(
            int idVenta,
            int idCliente,
            string metodoPago,
            List<DetalleVentaInputDto> detalles,
            int idUsuarioEditor)
        {
            return _ventaService.Actualizar(
                idVenta,
                idCliente,
                metodoPago,
                detalles,
                idUsuarioEditor);
        }

        public DataTable ObtenerVentas(string filtro)
        => _ventaService.ObtenerTodos(filtro);

        public Venta? ObtenerVentaPorId(int id)
            => _ventaService.ObtenerPorId(id);

        public List<DetalleVenta> ObtenerDetalles(int idVenta)
            => _ventaService.ObtenerDetallesPorVenta(idVenta);
    }
}
