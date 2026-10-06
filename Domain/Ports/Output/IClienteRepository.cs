using ProyectoArqSoft.Domain.Models;

namespace ProyectoArqSoft.Application.Ports.Output
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        int Count();
    }
}
