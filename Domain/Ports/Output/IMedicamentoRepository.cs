using ProyectoArqSoft.Domain.Models;
using System.Data;

namespace ProyectoArqSoft.Application.Ports.Output
{
    public interface IMedicamentoRepository : IRepository<Medicamento>
    {
        DataTable GetDestacados();
        int Count();
        int UpdateStock(int idMedicamento, int cantidad, bool esEntrada, int idUsuario);
    }
}
