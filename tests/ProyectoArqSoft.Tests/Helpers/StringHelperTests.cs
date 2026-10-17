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


    [Theory]
    [InlineData(null, "")]
    [InlineData("  CI\t 123\r\nLP  ", "CI123LP")]
    public void QuitarEspacios_AusenciaYTextoMixto_DevuelveValorNormalizado(string? texto, string esperado)
    {
        Assert.Equal(esperado, StringHelper.QuitarEspacios(texto));
    }


    [Theory]
    [InlineData(null, "")]
    [InlineData("  Ana\t \r\nMaria  ", "Ana Maria")]
    public void LimpiarTexto_AusenciaYTextoMixto_DevuelveValorNormalizado(string? texto, string esperado)
    {
        Assert.Equal(esperado, StringHelper.LimpiarTexto(texto));
    }


    [Theory]
    [InlineData(null, "")]
    [InlineData("  ácido\t fólico  ", "ÁCIDO FÓLICO")]
    public void LimpiarTextoMayus_AusenciaYTextoMixto_DevuelveValorNormalizado(string? texto, string esperado)
    {
        Assert.Equal(esperado, StringHelper.LimpiarTextoMayus(texto));
    }


    [Theory]
    [InlineData(null, "")]
    [InlineData("  ÁCIDO\t FÓLICO  ", "ácido fólico")]
    public void LimpiarTextoMinus_AusenciaYTextoMixto_DevuelveValorNormalizado(string? texto, string esperado)
    {
        Assert.Equal(esperado, StringHelper.LimpiarTextoMinus(texto));
    }


    [Theory]
    [InlineData(null, "")]
    [InlineData("CI 12-3A\t.4", "1234")]
    public void SoloNumeros_AusenciaYTextoMixto_DevuelveValorNormalizado(string? texto, string esperado)
    {
        Assert.Equal(esperado, StringHelper.SoloNumeros(texto));
    }


    [Theory]
    [InlineData(null, "")]
    [InlineData(" 12\t3 lp\r\n ", "123LP")]
    public void LimpiarCI_AusenciaYTextoMixto_DevuelveValorNormalizado(string? texto, string esperado)
    {
        Assert.Equal(esperado, StringHelper.LimpiarCI(texto));
    }

}
