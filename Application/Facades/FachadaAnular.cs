using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Domain.Validators;

namespace ProyectoArqSoft.Application.Facades
{
    public class FachadaAnular
    {
        private readonly IVentaService _ventaService;

        public FachadaAnular(IVentaService ventaService)
        {
            _ventaService = ventaService;
        }

        public Result AnularVenta(
            int idVenta,
            int idUsuarioEditor)
        {
            return _ventaService.EliminarLogicamente(
                idVenta,
                idUsuarioEditor);
        }
    }
}
