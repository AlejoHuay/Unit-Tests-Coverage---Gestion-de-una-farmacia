using ProyectoArqSoft.Domain.Validators;
using Xunit;

namespace ProyectoArqSoft.Tests.Validators;

public class BioquimicoBusquedaValidacionTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validar_FiltroVacio_RetornaExito(string? filtro)
    {
        // Arrange
        var validador = new BioquimicoBusquedaValidacion();

        // Act
        var resultado = validador.Validar(filtro!);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(string.Empty, resultado.Error);
    }

    [Theory]
    [InlineData("ab", "El criterio debe tener al menos 3 caracteres.")]
    [InlineData("1234567890123456789012345678901234567890123456", "El criterio no puede tener más de 45 caracteres.")]
    [InlineData("Ana@", "Criterio inválido.")]
    [InlineData("Ana123", "Criterio inválido.")]
    [InlineData("123-456", "Criterio inválido.")]
    public void Validar_FiltroInvalido_RetornaError(string filtro, string errorEsperado)
    {
        // Arrange
        var validador = new BioquimicoBusquedaValidacion();

        // Act
        var resultado = validador.Validar(filtro);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(errorEsperado, resultado.Error);
    }

    [Theory]
    [InlineData("Ana Maria")]
    [InlineData("12345")]
    [InlineData("12345-6A")]
    [InlineData(" Ana   Maria ")]
    public void Validar_FiltroPermitido_RetornaExito(string filtro)
    {
        // Arrange
        var validador = new BioquimicoBusquedaValidacion();

        // Act
        var resultado = validador.Validar(filtro);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(string.Empty, resultado.Error);
    }
}
