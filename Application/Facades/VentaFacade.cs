using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Domain.DTOs;
using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;
using System.Data;

namespace ProyectoArqSoft.Application.Facades
{
    public class VentaFacade : IVentaFacade
    {
        private readonly FachadaVenta _fv;
        private readonly FachadaAnular _fa;
        private readonly FachadaActualizarStock _fasm;
        private readonly IClienteService _clienteService;

        public VentaFacade(
            FachadaVenta fv,
            FachadaAnular fa,
            FachadaActualizarStock fasm,
            IClienteService clienteService)
        {
            _fv = fv;
            _fa = fa;
            _fasm = fasm;
            _clienteService = clienteService;
        }

        public DataTable ObtenerVentas(string filtro)
            => _fv.ObtenerVentas(filtro);

        public Venta? ObtenerVentaPorId(int id)
            => _fv.ObtenerVentaPorId(id);

        public List<DetalleVenta> ObtenerDetalles(int idVenta)
            => _fv.ObtenerDetalles(idVenta);

        public Result CrearVenta(
            int idCliente,
            int idUsuario,
            string metodoPago,
            List<DetalleVentaInputDto> detalles)
            => _fv.RegistrarVenta(
                idCliente,
                idUsuario,
                metodoPago,
                detalles);

        public Result ActualizarVenta(
            int idVenta,
            int idCliente,
            string metodoPago,
            List<DetalleVentaInputDto> detalles,
            int idUsuarioEditor)
            => _fv.ActualizarVenta(
                idVenta,
                idCliente,
                metodoPago,
                detalles,
                idUsuarioEditor);

        public Result AnularVenta(
            int idVenta,
            int idUsuarioEditor)
            => _fa.AnularVenta(
                idVenta,
                idUsuarioEditor);

        public DataTable ObtenerClientes()
            => _clienteService.ObtenerTodos();

        public DataTable ObtenerMedicamentos()
            => _fasm.ObtenerMedicamentos();
    }
}
