using ProyectoArqSoft.Domain.DTOs;
using Xunit;

namespace ProyectoArqSoft.Tests.DTOs;

[Trait("Category", "Unit")]
public class UsuarioLoginRequestDtoTests
{
    [Fact]
    public void Constructor_YAsignacion_ConservanElContratoDeDatos()
    {
        var dto = new UsuarioLoginRequestDto();

        Assert.Equal(string.Empty, dto.EmailOUserName);
        Assert.Equal(string.Empty, dto.Password);

        dto.EmailOUserName = "ana@example.test";
        dto.Password = "clave-solo-prueba";

        Assert.Equal("ana@example.test", dto.EmailOUserName);
        Assert.Equal("clave-solo-prueba", dto.Password);
    }
}

