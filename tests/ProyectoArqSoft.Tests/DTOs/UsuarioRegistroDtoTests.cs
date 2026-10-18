using ProyectoArqSoft.Domain.DTOs;
using Xunit;

namespace ProyectoArqSoft.Tests.DTOs;

[Trait("Category", "Unit")]
public class UsuarioRegistroDtoTests
{
    [Fact]
    public void Constructor_YAsignacion_ConservanElContratoDeDatos()
    {
        var dto = new UsuarioRegistroDto();

        Assert.Equal(string.Empty, dto.Nombres);
        Assert.Equal(string.Empty, dto.ApellidoPaterno);
        Assert.Equal(string.Empty, dto.Ci);
        Assert.Equal(string.Empty, dto.CiExtencion);
        Assert.Equal(string.Empty, dto.Telefono);
        Assert.Equal(string.Empty, dto.Email);
        Assert.Equal(string.Empty, dto.UserName);
        Assert.Equal(string.Empty, dto.Password);
        Assert.Null(dto.ApellidoMaterno);

        dto.Nombres = "Ana";
        dto.ApellidoPaterno = "Rojas";
        dto.ApellidoMaterno = "Lima";
        dto.Ci = "1234567";
        dto.CiExtencion = "LP";
        dto.Telefono = "76543210";
        dto.Email = "ana@example.test";
        dto.UserName = "ana";
        dto.Password = "clave-solo-prueba";

        Assert.Equal("Ana", dto.Nombres);
        Assert.Equal("Rojas", dto.ApellidoPaterno);
        Assert.Equal("Lima", dto.ApellidoMaterno);
        Assert.Equal("1234567", dto.Ci);
        Assert.Equal("LP", dto.CiExtencion);
        Assert.Equal("76543210", dto.Telefono);
        Assert.Equal("ana@example.test", dto.Email);
        Assert.Equal("ana", dto.UserName);
        Assert.Equal("clave-solo-prueba", dto.Password);
    }
}

