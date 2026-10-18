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

        [Fact]
        public void EnviarCorreoActivacionCuenta_ConPuertoSmtpInvalido_DevuelveError()
        {
            // Arrange - C7:
            // 1 -> 2 -> 3 -> 5 -> 7 -> 9 -> 11
            // -> 12 -> 13 -> 16 -> 17 -> 18

            // Configuracion ficticia, completamente independiente
            // de appsettings.json y de las credenciales reales.
            var valores = new Dictionary<string, string?>
            {
                ["SmtpSettings:Host"] = "127.0.0.1",
                ["SmtpSettings:Port"] = "65536",
                ["SmtpSettings:RemitenteNombre"] = "VitalCare Pruebas",
                ["SmtpSettings:RemitenteEmail"] = "pruebas@example.test",
                ["SmtpSettings:Password"] = "ficticia",
                ["SmtpSettings:UseSsl"] = "true"
            };

            IConfiguration configuracion = new ConfigurationBuilder()
                .AddInMemoryCollection(valores)
                .Build();

            var service = new EmailService(configuracion);

            // Act
            var resultado = service.EnviarCorreoActivacionCuenta(
                "ana@example.test",
                "Ana",
                "ana01",
                "Temporal123",
                "https://example.test/activar"
            );

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.StartsWith(
                "No se pudo enviar el correo",
                resultado.Error
            );

            // El nombre del parametro que causa el error
            // debe identificar el puerto SMTP.
            Assert.Contains(
                "port",
                resultado.Error.ToLowerInvariant()
            );
        }


        [Fact]
        public void EnviarCorreoActivacionCuenta_ConServidorSmtpNoDisponible_DevuelveError()
        {
            // Arrange - C8:
            // 1 -> 2 -> 3 -> 5 -> 7 -> 9 -> 11 -> 12 -> 13
            // -> 14 -> 16 -> 17 -> 18

            // Obtener un puerto local libre y cerrarlo.
            // No se utiliza ningun servidor SMTP externo.
            var listener = new System.Net.Sockets.TcpListener(
                System.Net.IPAddress.Loopback, 0);

            listener.Start();
            int puertoLocal;
            try
            {
                puertoLocal =
                    ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
            }
            finally
            {
                listener.Stop();
            }

            var valores = new Dictionary<string, string?>
            {
                ["SmtpSettings:Host"] = "127.0.0.1",
                ["SmtpSettings:Port"] = puertoLocal.ToString(),
                ["SmtpSettings:RemitenteNombre"] = "VitalCare Pruebas",
                ["SmtpSettings:RemitenteEmail"] = "pruebas@example.test",
                ["SmtpSettings:Password"] = "ficticia",
                ["SmtpSettings:UseSsl"] = "true"
            };

            IConfiguration configuracion = new ConfigurationBuilder()
                .AddInMemoryCollection(valores)
                .Build();

            var service = new EmailService(configuracion);

            // Act
            var resultado = service.EnviarCorreoActivacionCuenta(
                "ana@example.test",
                "Ana",
                "ana01",
                "Temporal123",
                "https://example.test/activar"
            );

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.StartsWith(
                "No se pudo enviar el correo",
                resultado.Error
            );

            // Confirmar el mensaje observado durante el fallo de envio local.
            Assert.Contains(
                "Failure sending mail.",
                resultado.Error
            );
        }
}
