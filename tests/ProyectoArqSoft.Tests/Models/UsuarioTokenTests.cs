using ProyectoArqSoft.Domain.Models;
using Xunit;

namespace ProyectoArqSoft.Tests.Models;

[Trait("Category", "Unit")]
public class UsuarioTokenTests
{
    [Fact]
    public void ConstructorVacio_InicializaEstadoYValoresOpcionales()
    {
        var antes = DateTime.UtcNow;
        var modelo = new UsuarioToken();

        Assert.Equal(string.Empty, modelo.TokenHash);
        Assert.Equal(string.Empty, modelo.TipoToken);
        Assert.Equal(0, modelo.Revocado);
        Assert.Equal(0, modelo.Usado);
        Assert.Null(modelo.FechaUso);
        Assert.Null(modelo.FechaRevocacion);
        Assert.Equal(DateTimeKind.Utc, modelo.FechaCreacion.Kind);
        Assert.InRange(modelo.FechaCreacion, antes, DateTime.UtcNow);
    }

    [Fact]
    public void ConstructorConDatos_ConservaDatosYPermiteActualizarMetadatos()
    {
        var modelo = new UsuarioToken(9, "hash-solo-prueba", "Activacion", new DateTime(2030, 1, 10, 12, 0, 0, DateTimeKind.Utc));

        Assert.Equal(9, modelo.UsuarioIdUsuario);
        Assert.Equal("hash-solo-prueba", modelo.TokenHash);
        Assert.Equal("Activacion", modelo.TipoToken);
        Assert.Equal(new DateTime(2030, 1, 10, 12, 0, 0, DateTimeKind.Utc), modelo.FechaExpiracion);
        Assert.Equal(0, modelo.Revocado);
        Assert.Equal(0, modelo.Usado);

        modelo.IdUsuarioToken = 7;
        modelo.FechaCreacion = new DateTime(2030, 1, 10, 11, 0, 0, DateTimeKind.Utc);
        modelo.Revocado = 1;
        modelo.Usado = 1;
        modelo.FechaUso = new DateTime(2030, 1, 10, 11, 5, 0, DateTimeKind.Utc);
        modelo.FechaRevocacion = new DateTime(2030, 1, 10, 11, 6, 0, DateTimeKind.Utc);

        Assert.Equal(7, modelo.IdUsuarioToken);
        Assert.Equal(new DateTime(2030, 1, 10, 11, 0, 0, DateTimeKind.Utc), modelo.FechaCreacion);
        Assert.Equal(1, modelo.Revocado);
        Assert.Equal(1, modelo.Usado);
        Assert.Equal(new DateTime(2030, 1, 10, 11, 5, 0, DateTimeKind.Utc), modelo.FechaUso);
        Assert.Equal(new DateTime(2030, 1, 10, 11, 6, 0, DateTimeKind.Utc), modelo.FechaRevocacion);
    }
}

