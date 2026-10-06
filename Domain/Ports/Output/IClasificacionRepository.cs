using ProyectoArqSoft.Domain.Models;

namespace ProyectoArqSoft.Application.Ports.Output
{
    public interface IClasificacionRepository : IRepository<Clasificacion>
    {
        bool TieneMedicamentosActivosAsociados(int idClasificacion);
        bool ExisteNombreActivo(string nombre);
        bool ExisteNombreActivoExcluyendoId(int idClasificacion, string nombre);
    }
}
