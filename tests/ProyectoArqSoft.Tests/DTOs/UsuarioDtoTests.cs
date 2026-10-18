using ProyectoArqSoft.Domain.DTOs;
using Xunit;

namespace ProyectoArqSoft.Tests.DTOs;

[Trait("Category", "Unit")]
public class UsuarioDtoTests
{
    [Fact]
    public void Constructor_YAsignacion_ConservanElContratoDeDatos()
    {
        var dto = new UsuarioDto();

        Assert.Equal(string.Empty, dto.Nombres);
        Assert.Equal(string.Empty, dto.ApellidoPaterno);
        Assert.Equal(string.Empty, dto.Ci);
        Assert.Equal(string.Empty, dto.CiExtencion);
        Assert.Equal(string.Empty, dto.Telefono);
        Assert.Equal(string.Empty, dto.Email);
        Assert.Equal(string.Empty, dto.UserName);
        Assert.Equal(string.Empty, dto.Role);
        Assert.Equal(1, dto.Activo);
        Assert.Equal(1, dto.MustChangePassword);
        Assert.Null(dto.ApellidoMaterno);

        dto.IdUsuario = 9;
        dto.Nombres = "Ana";
        dto.ApellidoPaterno = "Rojas";
        dto.ApellidoMaterno = "Lima";
        dto.Ci = "1234567";
        dto.CiExtencion = "LP";
        dto.Telefono = "76543210";
        dto.Activo = 0;
        dto.Email = "ana@example.test";
        dto.UserName = "ana";
        dto.Role = "Bioquimico";
        dto.MustChangePassword = 0;

        Assert.Equal(9, dto.IdUsuario);
        Assert.Equal("Ana", dto.Nombres);
        Assert.Equal("Rojas", dto.ApellidoPaterno);
        Assert.Equal("Lima", dto.ApellidoMaterno);
        Assert.Equal("1234567", dto.Ci);
        Assert.Equal("LP", dto.CiExtencion);
        Assert.Equal("76543210", dto.Telefono);
        Assert.Equal(0, dto.Activo);
        Assert.Equal("ana@example.test", dto.Email);
        Assert.Equal("ana", dto.UserName);
        Assert.Equal("Bioquimico", dto.Role);
        Assert.Equal(0, dto.MustChangePassword);
    }
}

