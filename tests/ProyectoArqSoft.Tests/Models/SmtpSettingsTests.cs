using ProyectoArqSoft.Domain.Models;
using Xunit;

namespace ProyectoArqSoft.Tests.Models;

[Trait("Category", "Unit")]
public class SmtpSettingsTests
{
    [Fact]
    public void Constructor_YAsignacion_ConservanElContratoDeDatos()
    {
        var modelo = new SmtpSettings();

        Assert.Equal(string.Empty, modelo.Host);
        Assert.Equal(string.Empty, modelo.RemitenteNombre);
        Assert.Equal(string.Empty, modelo.RemitenteEmail);
        Assert.Equal(string.Empty, modelo.Password);
        Assert.Equal(0, modelo.Port);
        Assert.False(modelo.UseSsl);

        modelo.Host = "smtp.example.test";
        modelo.Port = 587;
        modelo.RemitenteNombre = "VitalCare";
        modelo.RemitenteEmail = "correo@example.test";
        modelo.Password = "clave-solo-prueba";
        modelo.UseSsl = true;

        Assert.Equal("smtp.example.test", modelo.Host);
        Assert.Equal(587, modelo.Port);
        Assert.Equal("VitalCare", modelo.RemitenteNombre);
        Assert.Equal("correo@example.test", modelo.RemitenteEmail);
        Assert.Equal("clave-solo-prueba", modelo.Password);
        Assert.True(modelo.UseSsl);
    }
}

