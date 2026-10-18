using ProyectoArqSoft.Domain.DTOs;
using ProyectoArqSoft.Domain.Validators;
using Xunit;

namespace ProyectoArqSoft.Tests.Validators;

public class MovimientoStockValidacionTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validar_IdMedicamentoNoPositivo_DevuelveError(int id)
    {
        // Arrange
        var movimiento = MovimientoValido();
        movimiento.IdMedicamento = id;
        var validador = new MovimientoStockValidacion();

        // Act
        var resultado = validador.Validar(movimiento);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El medicamento no es válido.", resultado.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validar_CantidadNoPositiva_DevuelveError(int cantidad)
    {
        // Arrange
        var movimiento = MovimientoValido();
        movimiento.Cantidad = cantidad;
        var validador = new MovimientoStockValidacion();

        // Act
        var resultado = validador.Validar(movimiento);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("La cantidad del movimiento debe ser mayor a cero.", resultado.Error);
    }

    [Fact]
    public void Validar_CantidadSobreMaximo_DevuelveError()
    {
        // Arrange
        var movimiento = MovimientoValido();
        movimiento.Cantidad = 100001;
        var validador = new MovimientoStockValidacion();

        // Act
        var resultado = validador.Validar(movimiento);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("La cantidad del movimiento no puede ser mayor a 100000 unidades.", resultado.Error);
    }

    [Fact]
    public void Validar_StockActualNegativo_DevuelveError()
    {
        // Arrange
        var movimiento = MovimientoValido();
        movimiento.StockActual = -1;
        var validador = new MovimientoStockValidacion();

        // Act
        var resultado = validador.Validar(movimiento);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El stock actual del medicamento no puede ser negativo.", resultado.Error);
    }

    [Fact]
    public void Validar_SalidaMayorAlStock_DevuelveError()
    {
        // Arrange
        var movimiento = MovimientoValido();
        movimiento.EsEntrada = false;
        movimiento.Cantidad = 11;
        movimiento.StockActual = 10;
        var validador = new MovimientoStockValidacion();

        // Act
        var resultado = validador.Validar(movimiento);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("Stock insuficiente para realizar la venta.", resultado.Error);
    }

    [Fact]
    public void Validar_SalidaIgualAlStock_Acepta()
    {
        // Arrange
        var movimiento = MovimientoValido();
        movimiento.EsEntrada = false;
        movimiento.Cantidad = movimiento.StockActual;
        var validador = new MovimientoStockValidacion();

        // Act
        var resultado = validador.Validar(movimiento);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(string.Empty, resultado.Error);
    }

    [Fact]
    public void Validar_EntradaAceptaCantidadMayorAlStockActual()
    {
        // Arrange
        var movimiento = MovimientoValido();
        movimiento.EsEntrada = true;
        movimiento.Cantidad = 100000;
        movimiento.StockActual = 0;
        var validador = new MovimientoStockValidacion();

        // Act
        var resultado = validador.Validar(movimiento);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(string.Empty, resultado.Error);
    }

    [Fact]
    public void Validar_VariosErrores_DevuelveElPrimero()
    {
        // Arrange
        var movimiento = MovimientoValido();
        movimiento.IdMedicamento = 0;
        movimiento.Cantidad = 0;
        var validador = new MovimientoStockValidacion();

        // Act
        var resultado = validador.Validar(movimiento);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El medicamento no es válido.", resultado.Error);
    }

    private static MovimientoStockDto MovimientoValido() => new()
    {
        IdMedicamento = 1,
        Cantidad = 5,
        StockActual = 10,
        EsEntrada = false,
        IdUsuario = 1
    };
}
