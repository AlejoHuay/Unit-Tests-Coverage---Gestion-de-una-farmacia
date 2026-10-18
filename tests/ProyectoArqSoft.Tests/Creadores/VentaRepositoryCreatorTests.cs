using ProyectoArqSoft.Infrastructure.Creadores;
using ProyectoArqSoft.Infrastructure.Persistence.Repositories;
using Xunit;

namespace ProyectoArqSoft.Tests.Creadores;

public class VentaRepositoryCreatorTests
{
    [Fact]
    public void CreateRepo_CreaRepositorioDeVentaSinAbrirConexion()
    {
        // Arrange
        var dobles = new Infrastructure.DatabaseMocks();
        var creator = new VentaRepositoryCreator(dobles.Database);

        // Act
        var repository = creator.CreateRepo();

        // Assert
        Assert.IsType<VentaRepository>(repository);
        Assert.Empty(dobles.Events);
        Assert.Equal(System.Data.ConnectionState.Closed, dobles.State);
    }
}
