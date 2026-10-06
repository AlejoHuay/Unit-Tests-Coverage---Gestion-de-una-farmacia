using ProyectoArqSoft.Application.Ports.Output;

namespace ProyectoArqSoft.Infrastructure.Creadores
{
    public abstract class RepositoryCreator<T> //Clase creadora
    {
        public abstract IRepository<T> CreateRepo();

    }
}

