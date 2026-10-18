using ProyectoArqSoft.Domain.Models;
using Xunit;

namespace ProyectoArqSoft.Tests.Models;

[Trait("Category", "Unit")]
public class ClienteTests
{
    [Fact]
    public void ConstructorVacio_InicializaEstadoYValoresOpcionales()
    {
        var modelo = new Cliente();

        Assert.Equal(1, modelo.Estado);
        Assert.Equal(string.Empty, modelo.Nit);
        Assert.Equal(string.Empty, modelo.RazonSocial);
        Assert.Equal(string.Empty, modelo.CorreoElectronico);
        Assert.Null(modelo.UltimaActualizacion);
        Assert.Null(modelo.IdUsuario);
    }

    [Fact]
    public void ConstructorConDatos_ConservaDatosYPermiteActualizarMetadatos()
    {
        var modelo = new Cliente("123456", "Farmacia Central", "cliente@example.test");

        Assert.Equal("123456", modelo.Nit);
        Assert.Equal("Farmacia Central", modelo.RazonSocial);
        Assert.Equal("cliente@example.test", modelo.CorreoElectronico);

        modelo.IdCliente = 7;
        modelo.Estado = 0;
        modelo.FechaRegistro = new DateTime(2026, 1, 10);
        modelo.UltimaActualizacion = new DateTime(2026, 1, 11);
        modelo.IdUsuario = 9;

        Assert.Equal(7, modelo.IdCliente);
        Assert.Equal(0, modelo.Estado);
        Assert.Equal(new DateTime(2026, 1, 10), modelo.FechaRegistro);
        Assert.Equal(new DateTime(2026, 1, 11), modelo.UltimaActualizacion);
        Assert.Equal(9, modelo.IdUsuario);
    }
}

