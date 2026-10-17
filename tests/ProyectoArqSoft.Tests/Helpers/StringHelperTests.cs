using ProyectoArqSoft.Infrastructure.Helpers;
using Xunit;

namespace ProyectoArqSoft.Tests.Helpers;

public class StringHelperTests
{
    // Ejemplo inicial: valida un comportamiento real sin base de datos ni servidor.
    [Fact]
    public void Limpiar_ConEspaciosEnLosExtremos_DevuelveTextoSinEsosEspacios()
    {
        var resultado = StringHelper.Limpiar("  Paracetamol  ");

        Assert.Equal("Paracetamol", resultado);
    }

    [Fact]
    public void Limpiar_ConNull_DevuelveVacio()
    {
        Assert.Empty(StringHelper.Limpiar(null));
    }


    [Fact]
    public void Limpiar_ConEspaciosInteriores_NoLosColapsa()
    {
        Assert.Equal("Ana   Maria", StringHelper.Limpiar(" Ana   Maria "));
    }


    [Theory]
    [InlineData(null, "")]
    [InlineData("  Ana\t \r\nMaria  ", "Ana Maria")]
    public void LimpiarEspacios_AusenciaYTextoMixto_DevuelveValorNormalizado(string? texto, string esperado)
    {
        Assert.Equal(esperado, StringHelper.LimpiarEspacios(texto));
    }

}
