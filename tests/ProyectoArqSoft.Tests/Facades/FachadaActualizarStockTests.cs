using Moq;
using ProyectoArqSoft.Application.Facades;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Domain.Validators;
using Xunit;

namespace ProyectoArqSoft.Tests.Facades;

[Trait("Category", "Unit")]
public class FachadaActualizarStockTests
{
    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void ActualizarStock_EnviaArgumentosSinModificarYConservaResultado(bool esEntrada, bool exito)
    {
        // Valores distintos permiten detectar argumentos intercambiados.
        const int idMedicamento = 17;
        const int cantidad = 4;
        const int idUsuario = 9;
        var esperado = exito ? Result.Ok() : Result.Fail("No se pudo actualizar el stock.");
        var servicio = new Mock<IMedicamentoService>(MockBehavior.Strict);
        servicio.Setup(s => s.UpdateStock(idMedicamento, cantidad, esEntrada, idUsuario))
            .Returns(esperado);
        var fachada = new FachadaActualizarStock(servicio.Object);

        var resultado = fachada.ActualizarStock(idMedicamento, cantidad, esEntrada, idUsuario);

        Assert.Same(esperado, resultado);
        servicio.Verify(s => s.UpdateStock(idMedicamento, cantidad, esEntrada, idUsuario), Times.Once);
        servicio.VerifyNoOtherCalls();
    }
}
