using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using Microsoft.Extensions.Configuration;
using ProyectoArqSoft.Application.Services;
using Xunit;

namespace ProyectoArqSoft.SmtpIntegrationTests
{
    public class EmailServiceSmtpTests
    {
        [Fact]
        [Trait("Category", "SmtpIntegration")]
        public void EnviarCorreoActivacionCuenta_ConSmtpLocal_DevuelveExito()
        {
            // C9: 1 -> 2 -> 3 -> 5 -> 7 -> 9 -> 11 -> 12
            //     -> 13 -> 14 -> 15 -> 18

            string? huellaEsperada =
                Environment.GetEnvironmentVariable(
                    "VITALCARE_SMTP_TEST_THUMBPRINT");

            Assert.False(
                string.IsNullOrWhiteSpace(huellaEsperada),
                "Configura VITALCARE_SMTP_TEST_THUMBPRINT para ejecutar C9."
            );

            huellaEsperada = huellaEsperada.Trim();

            var anterior =
                ServicePointManager.ServerCertificateValidationCallback;

            try
            {
                ServicePointManager.ServerCertificateValidationCallback =
                    (sender, certificate, chain, errors) =>
                    {
                        if (certificate is null)
                            return false;

                        bool huellaCorrecta = string.Equals(
                            certificate.GetCertHashString(),
                            huellaEsperada,
                            StringComparison.OrdinalIgnoreCase
                        );

                        bool nombreCorrecto = certificate.Subject.Contains(
                            "CN=localhost",
                            StringComparison.OrdinalIgnoreCase
                        );

                        bool erroresPermitidos =
                            (errors & ~SslPolicyErrors.RemoteCertificateChainErrors)
                            == SslPolicyErrors.None;

                        return huellaCorrecta &&
                               nombreCorrecto &&
                               erroresPermitidos;
                    };

                var valores = new Dictionary<string, string?>
                {
                    ["SmtpSettings:Host"] = "localhost",
                    ["SmtpSettings:Port"] = "2525",
                    ["SmtpSettings:RemitenteNombre"] = "VitalCare Pruebas",
                    ["SmtpSettings:RemitenteEmail"] = "pruebas@example.test",
                    ["SmtpSettings:Password"] = "ficticia",
                    ["SmtpSettings:UseSsl"] = "true"
                };

                IConfiguration configuracion = new ConfigurationBuilder()
                    .AddInMemoryCollection(valores)
                    .Build();

                var servicio = new EmailService(configuracion);

                // Act
                var resultado = servicio.EnviarCorreoActivacionCuenta(
                    "destinatario@example.test",
                    "Ana",
                    "ana01",
                    "Temporal123",
                    "https://example.test/activar"
                );

                // Assert
                Assert.True(resultado.IsSuccess, resultado.Error);
            }
            finally
            {
                ServicePointManager.ServerCertificateValidationCallback =
                    anterior;
            }
        }
    }
}