using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;
using Xunit;

namespace ProyectoArqSoft.Tests.Validators;

public class MedicamentoValidacionTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validar_NombreVacio_DevuelveError(string? nombre)
    {
        // Arrange
        var medicamento = MedicamentoValido();
        medicamento.Nombre = nombre!;
        var validador = new MedicamentoValidacion();

        // Act
        var resultado = validador.Validar(medicamento);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El nombre es obligatorio.", resultado.Error);
    }

    [Theory]
    [InlineData("AB")]
    [InlineData("XX!!")]
    [InlineData("XXX")]
    public void Validar_NombreConFormatoInvalido_DevuelveError(string nombre)
    {
        // Arrange
        var medicamento = MedicamentoValido();
        medicamento.Nombre = nombre;
        var validador = new MedicamentoValidacion();

        // Act
        var resultado = validador.Validar(medicamento);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El nombre contiene caracteres inválidos o no tiene un formato correcto.", resultado.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validar_PresentacionVacia_DevuelveError(string presentacion)
    {
        // Arrange
        var medicamento = MedicamentoValido();
        medicamento.Presentacion = presentacion;
        var validador = new MedicamentoValidacion();

        // Act
        var resultado = validador.Validar(medicamento);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("La presentación es obligatoria.", resultado.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validar_ClasificacionNoPositiva_DevuelveError(int id)
    {
        // Arrange
        var medicamento = MedicamentoValido();
        medicamento.IdClasificacion = id;
        var validador = new MedicamentoValidacion();

        // Act
        var resultado = validador.Validar(medicamento);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("La clasificación es obligatoria.", resultado.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validar_ConcentracionVacia_DevuelveError(string concentracion)
    {
        // Arrange
        var medicamento = MedicamentoValido();
        medicamento.Concentracion = concentracion;
        var validador = new MedicamentoValidacion();

        // Act
        var resultado = validador.Validar(medicamento);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("La concentración es obligatoria.", resultado.Error);
    }

    [Theory]
    [InlineData("500mg/")]
    [InlineData("mg 500")]
    public void Validar_ConcentracionConFormatoInvalido_DevuelveError(string concentracion)
    {
        // Arrange
        var medicamento = MedicamentoValido();
        medicamento.Concentracion = concentracion;
        var validador = new MedicamentoValidacion();

        // Act
        var resultado = validador.Validar(medicamento);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("La concentración no tiene un formato válido. Ejemplos: 500 mg, 250 mg/5ml, 0.9 %.", resultado.Error);
    }

    [Theory]
    [InlineData("500 mg")]
    [InlineData("250 mg/5ml")]
    [InlineData("0.9 %")]
    [InlineData("1.25 G/2 L")]
    public void Validar_ConcentracionConFormatoAdmitido_Acepta(string concentracion)
    {
        // Arrange
        var medicamento = MedicamentoValido();
        medicamento.Concentracion = concentracion;
        var validador = new MedicamentoValidacion();

        // Act
        var resultado = validador.Validar(medicamento);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(string.Empty, resultado.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validar_PrecioNoPositivo_DevuelveError(decimal precio)
    {
        // Arrange
        var medicamento = MedicamentoValido();
        medicamento.Precio = precio;
        var validador = new MedicamentoValidacion();

        // Act
        var resultado = validador.Validar(medicamento);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El precio debe ser mayor a 0 Bs.", resultado.Error);
    }

    [Fact]
    public void Validar_PrecioMayorAlMaximo_DevuelveError()
    {
        // Arrange
        var medicamento = MedicamentoValido();
        medicamento.Precio = 1000.01m;
        var validador = new MedicamentoValidacion();

        // Act
        var resultado = validador.Validar(medicamento);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El precio no puede ser mayor a 1000 Bs.", resultado.Error);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1000)]
    public void Validar_PrecioEnLimites_Acepta(decimal precio)
    {
        // Arrange
        var medicamento = MedicamentoValido();
        medicamento.Precio = precio;
        var validador = new MedicamentoValidacion();

        // Act
        var resultado = validador.Validar(medicamento);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(string.Empty, resultado.Error);
    }

    [Fact]
    public void Validar_PrecioCero_DevuelveError()
    {
        // Arrange
        var medicamento = MedicamentoValido();
        medicamento.Precio = 0;
        var validador = new MedicamentoValidacion();

        // Act
        var resultado = validador.Validar(medicamento);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El precio debe ser mayor a 0 Bs.", resultado.Error);
    }

    [Fact]
    public void Validar_StockNegativo_DevuelveError()
    {
        // Arrange
        var medicamento = MedicamentoValido();
        medicamento.Stock = -1;
        var validador = new MedicamentoValidacion();

        // Act
        var resultado = validador.Validar(medicamento);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El stock no puede ser negativo.", resultado.Error);
    }

    [Fact]
    public void Validar_StockSobreMaximo_DevuelveError()
    {
        // Arrange
        var medicamento = MedicamentoValido();
        medicamento.Stock = 100001;
        var validador = new MedicamentoValidacion();

        // Act
        var resultado = validador.Validar(medicamento);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El stock no puede ser mayor a 100000 items.", resultado.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100000)]
    public void Validar_StockEnLimites_Acepta(int stock)
    {
        // Arrange
        var medicamento = MedicamentoValido();
        medicamento.Stock = stock;
        var validador = new MedicamentoValidacion();

        // Act
        var resultado = validador.Validar(medicamento);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(string.Empty, resultado.Error);
    }

    [Fact]
    public void Validar_VariosErrores_DevuelveElPrimero()
    {
        // Arrange
        var medicamento = MedicamentoValido();
        medicamento.Nombre = "";
        medicamento.Presentacion = "";
        medicamento.Precio = 0;
        var validador = new MedicamentoValidacion();

        // Act
        var resultado = validador.Validar(medicamento);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El nombre es obligatorio.", resultado.Error);
    }

    private static Medicamento MedicamentoValido() => new()
    {
        Nombre = "Paracetamol",
        Presentacion = "Tableta",
        IdClasificacion = 1,
        Concentracion = "500 mg",
        Precio = 10m,
        Stock = 20
    };
}
