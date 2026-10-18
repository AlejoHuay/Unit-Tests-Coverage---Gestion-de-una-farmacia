using ProyectoArqSoft.Domain.DTOs;
using Xunit;

namespace ProyectoArqSoft.Tests.DTOs;

[Trait("Category", "Unit")]
public class MovimientoStockDtoTests
{
    [Fact]
    public void Constructor_YAsignacion_ConservanElContratoDeDatos()
    {
        var dto = new MovimientoStockDto();

        Assert.False(dto.EsEntrada);
        Assert.Equal(0, dto.StockActual);

        dto.IdMedicamento = 17;
        dto.Cantidad = 5;
        dto.EsEntrada = true;
        dto.IdUsuario = 9;
        dto.StockActual = 40;

        Assert.Equal(17, dto.IdMedicamento);
        Assert.Equal(5, dto.Cantidad);
        Assert.True(dto.EsEntrada);
        Assert.Equal(9, dto.IdUsuario);
        Assert.Equal(40, dto.StockActual);
    }
}

