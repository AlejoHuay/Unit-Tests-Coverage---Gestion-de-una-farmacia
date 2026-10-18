using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;
using Xunit;

namespace ProyectoArqSoft.Tests.Validators;

public class ClasificacionValidacionTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validar_NombreVacio_DevuelveError(string? nombre)
    {
        // Arrange
        var clasificacion = ClasificacionValida();
        clasificacion.Nombre = nombre!;
        var validador = new ClasificacionValidacion();

        // Act
        var resultado = validador.Validar(clasificacion);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El nombre de la clasificación es obligatorio.", resultado.Error);
    }

    [Theory]
    [InlineData("Ab")]
    [InlineData("1234567890123456789012345678901234567890123456")]
    public void Validar_NombreFueraDeLimites_DevuelveError(string nombre)
    {
        // Arrange
        var clasificacion = ClasificacionValida();
        clasificacion.Nombre = nombre;
        var validador = new ClasificacionValidacion();

        // Act
        var resultado = validador.Validar(clasificacion);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El nombre debe tener entre 3 y 45 caracteres.", resultado.Error);
    }

    [Fact]
    public void Validar_NombreConCaracterNoPermitido_DevuelveError()
    {
        // Arrange
        var clasificacion = ClasificacionValida();
        clasificacion.Nombre = "Analgesicos-Plus";
        var validador = new ClasificacionValidacion();

        // Act
        var resultado = validador.Validar(clasificacion);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El nombre contiene caracteres inválidos.", resultado.Error);
    }

    [Fact]
    public void Validar_NombreRepetido_DevuelveError()
    {
        // Arrange
        var clasificacion = ClasificacionValida();
        clasificacion.Nombre = "AAAA";
        var validador = new ClasificacionValidacion();

        // Act
        var resultado = validador.Validar(clasificacion);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El nombre no puede estar compuesto por un único carácter repetido.", resultado.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validar_OrigenVacio_DevuelveError(string? origen)
    {
        // Arrange
        var clasificacion = ClasificacionValida();
        clasificacion.Origen = origen!;
        var validador = new ClasificacionValidacion();

        // Act
        var resultado = validador.Validar(clasificacion);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El origen es obligatorio.", resultado.Error);
    }

    [Theory]
    [InlineData("Ab")]
    [InlineData("1234567890123456789012345678901234567890123456")]
    public void Validar_OrigenFueraDeLimites_DevuelveError(string origen)
    {
        // Arrange
        var clasificacion = ClasificacionValida();
        clasificacion.Origen = origen;
        var validador = new ClasificacionValidacion();

        // Act
        var resultado = validador.Validar(clasificacion);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El origen debe tener entre 3 y 45 caracteres.", resultado.Error);
    }

    [Fact]
    public void Validar_OrigenConCaracterNoPermitido_DevuelveError()
    {
        // Arrange
        var clasificacion = ClasificacionValida();
        clasificacion.Origen = "Natural-Plus";
        var validador = new ClasificacionValidacion();

        // Act
        var resultado = validador.Validar(clasificacion);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El origen contiene caracteres inválidos.", resultado.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validar_DescripcionVacia_DevuelveError(string? descripcion)
    {
        // Arrange
        var clasificacion = ClasificacionValida();
        clasificacion.Descripcion = descripcion!;
        var validador = new ClasificacionValidacion();

        // Act
        var resultado = validador.Validar(clasificacion);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("La descripción es obligatoria.", resultado.Error);
    }

    [Theory]
    [InlineData("Abcd")]
    [InlineData("12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901")]
    public void Validar_DescripcionFueraDeLimites_DevuelveError(string descripcion)
    {
        // Arrange
        var clasificacion = ClasificacionValida();
        clasificacion.Descripcion = descripcion;
        var validador = new ClasificacionValidacion();

        // Act
        var resultado = validador.Validar(clasificacion);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("La descripción debe tener entre 5 y 100 caracteres.", resultado.Error);
    }

    [Fact]
    public void Validar_CamposEnLimites_Acepta()
    {
        // Arrange
        var clasificacion = new Clasificacion(
            string.Concat(Enumerable.Repeat("AB", 22)) + "A",
            string.Concat(Enumerable.Repeat("BC", 22)) + "B",
            new string('C', 100));
        var validador = new ClasificacionValidacion();

        // Act
        var resultado = validador.Validar(clasificacion);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(string.Empty, resultado.Error);
    }

    [Fact]
    public void Validar_VariosErrores_DevuelveElPrimero()
    {
        // Arrange
        var clasificacion = ClasificacionValida();
        clasificacion.Nombre = "";
        clasificacion.Origen = "";
        clasificacion.Descripcion = "";
        var validador = new ClasificacionValidacion();

        // Act
        var resultado = validador.Validar(clasificacion);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal("El nombre de la clasificación es obligatorio.", resultado.Error);
    }

    private static Clasificacion ClasificacionValida() => new("Analgesicos", "Sintetico", "Uso general");
}
