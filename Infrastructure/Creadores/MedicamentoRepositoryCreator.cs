using ProyectoArqSoft.Infrastructure.Persistence.Connection;
using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Application.Ports.Output;
using ProyectoArqSoft.Infrastructure.Persistence.Repositories;

namespace ProyectoArqSoft.Infrastructure.Creadores
{
    public class MedicamentoRepositoryCreator : IRepositoryCreator<Medicamento>
    {
        private readonly PostgresDatabase database;
        public MedicamentoRepositoryCreator(PostgresDatabase database) => this.database = database;

        public IRepository<Medicamento> CreateRepo()
        {
            return new MedicamentoRepository(database);
        }
    }
}
