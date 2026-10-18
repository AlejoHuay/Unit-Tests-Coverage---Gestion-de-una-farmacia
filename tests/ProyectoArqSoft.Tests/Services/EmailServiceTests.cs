using Microsoft.Extensions.Configuration;
using ProyectoArqSoft.Application.Services;
using ProyectoArqSoft.Domain.Validators;
using Xunit;

namespace ProyectoArqSoft.Tests.Services;

public class EmailServiceTests
{
    [Theory]
    [InlineData(null, "Ana", "ana", "Password123", "https://example.test/activate",
        "El correo destino es obligatorio.")]
    [InlineData("ana@example.test", "Ana", " ", "Password123", "https://example.test/activate",
        "El nombre de usuario es obligatorio para el correo.")]
    [InlineData("ana@example.test", "Ana", "ana", "", "https://example.test/activate",
        "La contraseña temporal es obligatoria para el correo.")]
    [InlineData("ana@example.test", "Ana", "ana", "Password123", " ",
        "El enlace de activación es obligatorio.")]
    public void EnviarCorreoActivacionCuenta_CampoObligatorioVacio_RetornaError(
        string? email,
        string nombres,
        string userName,
        string passwordTemporal,
        string enlace,
        string errorEsperado)
    {
        // Arrange
        var configuracion = new ConfigurationBuilder().Build();
        var servicio = new EmailService(configuracion);

        // Act
        Result resultado = servicio.EnviarCorreoActivacionCuenta(
            email!,
            nombres,
            userName,
            passwordTemporal,
            enlace);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(errorEsperado, resultado.Error);
    }

    [Fact]
    public void EnviarCorreoActivacionCuenta_ConfiguracionSmtpInvalida_RetornaErrorSinEnviarCorreo()
    {
        // Arrange
        var configuracion = new ConfigurationBuilder().Build();
        var servicio = new EmailService(configuracion);

        // Act
        Result resultado = servicio.EnviarCorreoActivacionCuenta(
            "ana@example.test",
            " Ana ",
            " ana ",
            " Password123 ",
            " https://example.test/activate ");

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.StartsWith("No se pudo enviar el correo electrónico. Detalle:", resultado.Error);
    }
}
