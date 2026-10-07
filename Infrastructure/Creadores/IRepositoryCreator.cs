using ProyectoArqSoft.Application.Ports.Output;

namespace ProyectoArqSoft.Infrastructure.Creadores
{
    public interface IRepositoryCreator<T>
    {
        IRepository<T> CreateRepo();

    }
}

