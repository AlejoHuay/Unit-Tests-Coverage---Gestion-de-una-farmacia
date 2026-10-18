using ProyectoArqSoft.Domain.DTOs;
using Xunit;

namespace ProyectoArqSoft.Tests.DTOs;

[Trait("Category", "Unit")]
public class UsuarioTokenGeneracionDtoTests
{
    [Fact]
    public void Constructor_YAsignacion_ConservanElContratoDeDatos()
    {
        var dto = new UsuarioTokenGeneracionDto();

        Assert.Equal(string.Empty, dto.TipoToken);
        Assert.Null(dto.UserName);
        Assert.Null(dto.Role);
        Assert.Equal(0, dto.MinutosExpiracion);

        dto.IdUsuario = 9;
        dto.TipoToken = "Activacion";
        dto.MinutosExpiracion = 30;
        dto.UserName = "ana";
        dto.Role = "Admin";

        Assert.Equal(9, dto.IdUsuario);
        Assert.Equal("Activacion", dto.TipoToken);
        Assert.Equal(30, dto.MinutosExpiracion);
        Assert.Equal("ana", dto.UserName);
        Assert.Equal("Admin", dto.Role);
    }
}

