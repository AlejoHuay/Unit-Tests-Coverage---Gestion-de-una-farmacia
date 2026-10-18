using ProyectoArqSoft.Domain.DTOs;
using Xunit;

namespace ProyectoArqSoft.Tests.DTOs;

[Trait("Category", "Unit")]
public class UsuarioActualizarDtoTests
{
    [Fact]
    public void Constructor_YAsignacion_ConservanElContratoDeDatos()
    {
        var antes = DateTime.Now;
        var dto = new UsuarioActualizarDto();

        Assert.Equal(string.Empty, dto.Email);
        Assert.Equal(string.Empty, dto.Role);
        Assert.Equal(string.Empty, dto.UserName);
        Assert.Equal(string.Empty, dto.Nombres);
        Assert.Equal(string.Empty, dto.ApellidoPaterno);
        Assert.Equal(string.Empty, dto.ApellidoMaterno);
        Assert.Equal(string.Empty, dto.Ci);
        Assert.Equal(string.Empty, dto.CiExtencion);
        Assert.Equal(string.Empty, dto.Telefono);
        Assert.Equal(1, dto.Activo);
        Assert.Equal(1, dto.MustChangePassword);
        Assert.InRange(dto.UltimaActualizacion, antes, DateTime.Now);

        dto.IdUsuario = 9;
        dto.Email = "ana@example.test";
        dto.Role = "Bioquimico";
        dto.Activo = 0;
        dto.UserName = "ana";
        dto.Nombres = "Ana";
        dto.ApellidoPaterno = "Rojas";
        dto.ApellidoMaterno = "Lima";
        dto.Ci = "1234567";
        dto.CiExtencion = "LP";
        dto.Telefono = "76543210";
        dto.UltimaActualizacion = new DateTime(2026, 1, 10, 12, 0, 0);
        dto.MustChangePassword = 0;

        Assert.Equal(9, dto.IdUsuario);
        Assert.Equal("ana@example.test", dto.Email);
        Assert.Equal("Bioquimico", dto.Role);
        Assert.Equal(0, dto.Activo);
        Assert.Equal("ana", dto.UserName);
        Assert.Equal("Ana", dto.Nombres);
        Assert.Equal("Rojas", dto.ApellidoPaterno);
        Assert.Equal("Lima", dto.ApellidoMaterno);
        Assert.Equal("1234567", dto.Ci);
        Assert.Equal("LP", dto.CiExtencion);
        Assert.Equal("76543210", dto.Telefono);
        Assert.Equal(new DateTime(2026, 1, 10, 12, 0, 0), dto.UltimaActualizacion);
        Assert.Equal(0, dto.MustChangePassword);
    }
}

