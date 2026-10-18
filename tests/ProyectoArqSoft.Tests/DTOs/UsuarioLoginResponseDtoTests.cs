using ProyectoArqSoft.Domain.DTOs;
using Xunit;

namespace ProyectoArqSoft.Tests.DTOs;

[Trait("Category", "Unit")]
public class UsuarioLoginResponseDtoTests
{
    [Fact]
    public void Constructor_YAsignacion_ConservanElContratoDeDatos()
    {
        var dto = new UsuarioLoginResponseDto();

        Assert.Equal(string.Empty, dto.UserName);
        Assert.Equal(string.Empty, dto.Role);
        Assert.Equal(string.Empty, dto.Token);
        Assert.False(dto.MustChangePassword);
        Assert.Equal(0, dto.ExpiraEn);

        dto.IdUsuario = 9;
        dto.UserName = "ana";
        dto.Role = "Admin";
        dto.MustChangePassword = true;
        dto.Token = "token-solo-prueba";
        dto.ExpiraEn = 3600;

        Assert.Equal(9, dto.IdUsuario);
        Assert.Equal("ana", dto.UserName);
        Assert.Equal("Admin", dto.Role);
        Assert.True(dto.MustChangePassword);
        Assert.Equal("token-solo-prueba", dto.Token);
        Assert.Equal(3600, dto.ExpiraEn);
    }
}

