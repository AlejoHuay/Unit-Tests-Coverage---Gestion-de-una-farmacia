using ProyectoArqSoft.Domain.Validators;
using Xunit;

namespace ProyectoArqSoft.Tests.Validators;

public class BioquimicoBusquedaValidacionTests
{
    private readonly BioquimicoBusquedaValidacion _validador = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t\r\n ")]
    public void Validar_SinCriterio_DevuelveExito(string? filtro)
    {
        // El helper admite null aunque la firma del validador no sea nullable.
        var resultado = _validador.Validar(filtro!);

        Assert.True(resultado.IsSuccess);
        Assert.Empty(resultado.Error);
    }

    [Theory]
    [InlineData("ab", "El criterio debe tener al menos 3 caracteres.")]
    [InlineData("Ana@", "Criterio inválido.")]
    public void Validar_ErrorGeneral_ConservaElMensaje(string filtro, string error)
    {
        var resultado = _validador.Validar(filtro);

        Assert.False(resultado.IsSuccess);
        Assert.Equal(error, resultado.Error);
    }

    [Theory]
    [InlineData(2, "El criterio debe tener al menos 3 caracteres.")]
    [InlineData(3, "")]
    [InlineData(45, "")]
    [InlineData(46, "El criterio no puede tener más de 45 caracteres.")]
    public void Validar_LimitesDeLongitud_EvaluaElTextoLimpio(int longitud, string error)
    {
        var filtro = "  " + new string('a', longitud) + "  ";

        var resultado = _validador.Validar(filtro);

        Assert.Equal(error.Length == 0, resultado.IsSuccess);
        Assert.Equal(error, resultado.Error);
    }

    [Theory]
    [InlineData("Ana Perez")]
    [InlineData("123456")]
    [InlineData("123-4A")]
    [InlineData(" \tAna   Perez\r\n ")]
    public void Validar_FormatoPermitido_DevuelveExito(string filtro)
    {
        var resultado = _validador.Validar(filtro);

        Assert.True(resultado.IsSuccess);
        Assert.Empty(resultado.Error);
    }

    [Theory]
    [InlineData("Ana123")]
    [InlineData("123-4a")]
    [InlineData("123-45A")]
    [InlineData("Ana-Perez")]
    [InlineData("José")]
    public void Validar_FormatoNoPermitidoPorRegex_DevuelveError(string filtro)
    {
        // Estos textos pasan el helper general y fallan en la expresión regular.
        var resultado = _validador.Validar(filtro);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("Criterio inválido.", resultado.Error);
    }
}
