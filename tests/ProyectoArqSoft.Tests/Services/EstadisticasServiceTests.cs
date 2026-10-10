using Moq;
using ProyectoArqSoft.Application.Ports.Output;
using ProyectoArqSoft.Application.Services;
using Xunit;

namespace ProyectoArqSoft.Tests.Services;

public class EstadisticasServiceTests
{
    [Fact]
    public void ObtenerEstadisticas_ConConteosConocidos_DevuelveCadaTotalCorrectamente()
    {
        // Preparar: valores distintos permiten detectar totales intercambiados.
        var medicamentoRepo = new Mock<IMedicamentoRepository>(MockBehavior.Strict);
        var clienteRepo = new Mock<IClienteRepository>(MockBehavior.Strict);
        var usuarioRepo = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        var ventaRepo = new Mock<IVentaRepository>(MockBehavior.Strict);

        medicamentoRepo.Setup(repo => repo.Count()).Returns(12);
        clienteRepo.Setup(repo => repo.Count()).Returns(7);
        usuarioRepo.Setup(repo => repo.Count()).Returns(3);
        ventaRepo.Setup(repo => repo.Count()).Returns(25);

        var servicio = new EstadisticasService(
            medicamentoRepo.Object,
            clienteRepo.Object,
            usuarioRepo.Object,
            ventaRepo.Object);

        // Actuar: recorrer el unico camino independiente del metodo.
        var resultado = servicio.ObtenerEstadisticas();

        // Comprobar: cada propiedad debe conservar el conteo de su repositorio.
        Assert.Equal(12, resultado.TotalMedicamentos);
        Assert.Equal(7, resultado.TotalClientes);
        Assert.Equal(3, resultado.TotalUsuarios);
        Assert.Equal(25, resultado.TotalVentas);
    }
}
