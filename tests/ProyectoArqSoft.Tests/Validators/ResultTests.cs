using ProyectoArqSoft.Domain.Validators;
using Xunit;

namespace ProyectoArqSoft.Tests.Validators;

[Trait("Category", "Unit")]
public class ResultTests
{
    [Theory]
    [InlineData(true, "Operacion completada")]
    [InlineData(false, "Dato invalido")]
    public void Constructor_ConservaEstadoYMensajeRecibidos(bool exito, string mensaje)
    {
        var resultado = new Result(exito, mensaje);

        Assert.Equal(exito, resultado.IsSuccess);
        Assert.Equal(mensaje, resultado.Error);
    }

    [Fact]
    public void Ok_DevuelveExitoSinError()
    {
        var resultado = Result.Ok();

        Assert.True(resultado.IsSuccess);
        Assert.Equal(string.Empty, resultado.Error);
    }

    [Fact]
    public void Fail_DevuelveFalloConElMensajeOriginal()
    {
        var resultado = Result.Fail("No existe el registro.");

        Assert.False(resultado.IsSuccess);
        Assert.Equal("No existe el registro.", resultado.Error);
    }
}

