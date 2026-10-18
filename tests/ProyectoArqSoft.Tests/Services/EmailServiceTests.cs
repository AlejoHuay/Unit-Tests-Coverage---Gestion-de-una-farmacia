using Microsoft.Extensions.Configuration;
using Moq;
using ProyectoArqSoft.Application.Services;
using Xunit;

namespace ProyectoArqSoft.Tests.Services
{
    public class EmailServiceTests
    {
        private static EmailService CrearServicio()
        {
            var section = new Mock<IConfigurationSection>();

            var configuration = new Mock<IConfiguration>();
            configuration
                .Setup(c => c.GetSection("SmtpSettings"))
                .Returns(section.Object);

            return new EmailService(configuration.Object);
        }

        [Fact]
        public void EnviarCorreoActivacionCuenta_ConCorreoVacio_DevuelveError()
        {
            // Arrange - C1: 1 -> 2 -> 3 -> 4 -> 18
            var service = CrearServicio();

            // Act
            var resultado = service.EnviarCorreoActivacionCuenta(
                "   ", "Ana", "ana01", "Temporal123",
                "https://example.test/activar");

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal("El correo destino es obligatorio.", resultado.Error);
        }

        [Fact]
        public void EnviarCorreoActivacionCuenta_ConUsuarioVacio_DevuelveError()
        {
            // Arrange - C2: 1 -> 2 -> 3 -> 5 -> 6 -> 18
            var service = CrearServicio();

            // Act
            var resultado = service.EnviarCorreoActivacionCuenta(
                "ana@example.test", "Ana", "   ", "Temporal123",
                "https://example.test/activar");

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "El nombre de usuario es obligatorio para el correo.",
                resultado.Error);
        }

        [Fact]
        public void EnviarCorreoActivacionCuenta_ConPasswordVacio_DevuelveError()
        {
            // Arrange - C3: 1 -> 2 -> 3 -> 5 -> 7 -> 8 -> 18
            var service = CrearServicio();

            // Act
            var resultado = service.EnviarCorreoActivacionCuenta(
                "ana@example.test", "Ana", "ana01", "   ",
                "https://example.test/activar");

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "La contraseña temporal es obligatoria para el correo.",
                resultado.Error);
        }

        [Fact]
        public void EnviarCorreoActivacionCuenta_ConEnlaceVacio_DevuelveError()
        {
            // Arrange - C4: 1 -> 2 -> 3 -> 5 -> 7 -> 9 -> 10 -> 18
            var service = CrearServicio();

            // Act
            var resultado = service.EnviarCorreoActivacionCuenta(
                "ana@example.test", "Ana", "ana01", "Temporal123",
                "   ");

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "El enlace de activación es obligatorio.",
                resultado.Error);
        }

        [Fact]
        public void EnviarCorreoActivacionCuenta_ConRemitenteInvalido_DevuelveError()
        {
            // Arrange - C6: 1 -> 2 -> 3 -> 5 -> 7 -> 9
            //               -> 11 -> 12 -> 16 -> 17 -> 18
            // La configuracion SMTP ficticia tiene remitente vacio.
            var service = CrearServicio();

            // Act
            var resultado = service.EnviarCorreoActivacionCuenta(
                "ana@example.test",
                "Ana",
                "ana01",
                "Temporal123",
                "https://example.test/activar"
            );

            // Assert
            // El servicio recibe un remitente vacio en SmtpSettings.
            // MailAddress debe rechazarlo antes de crear SmtpClient.
            Assert.Throws<ArgumentException>(() =>
                new System.Net.Mail.MailAddress(string.Empty, string.Empty));

            Assert.False(resultado.IsSuccess);
            Assert.StartsWith(
                "No se pudo enviar el correo",
                resultado.Error
            );
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
}