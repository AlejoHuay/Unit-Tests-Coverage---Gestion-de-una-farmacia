using ProyectoArqSoft.Domain.DTOs;
using Xunit;

namespace ProyectoArqSoft.Tests.DTOs;

[Trait("Category", "Unit")]
public class MedicamentoActualizarDtoTests
{
    [Fact]
    public void Constructor_YAsignacion_ConservanElContratoDeDatos()
    {
        var dto = new MedicamentoActualizarDto();

        Assert.Equal(string.Empty, dto.Nombre);
        Assert.Equal(string.Empty, dto.Presentacion);
        Assert.Equal(string.Empty, dto.Concentracion);

        dto.Id = 17;
        dto.Nombre = "Paracetamol";
        dto.Presentacion = "Tableta";
        dto.IdClasificacion = 3;
        dto.Concentracion = "500 mg";
        dto.Precio = 12.50m;
        dto.Stock = 40;
        dto.IdUsuario = 9;

        Assert.Equal(17, dto.Id);
        Assert.Equal("Paracetamol", dto.Nombre);
        Assert.Equal("Tableta", dto.Presentacion);
        Assert.Equal(3, dto.IdClasificacion);
        Assert.Equal("500 mg", dto.Concentracion);
        Assert.Equal(12.50m, dto.Precio);
        Assert.Equal(40, dto.Stock);
        Assert.Equal(9, dto.IdUsuario);
    }
}

