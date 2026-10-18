using ProyectoArqSoft.Application.Helpers;
using Xunit;

namespace ProyectoArqSoft.Tests.Helpers;

public class NumeroATextoConverterTests
{
    [Theory]
    [InlineData(0, "cero")]
    [InlineData(16, "dieciséis")]
    [InlineData(20, "veinte")]
    [InlineData(31, "treinta y uno")]
    [InlineData(100, "cien")]
    [InlineData(101, "ciento uno")]
    [InlineData(200, "doscientos")]
    [InlineData(2234, "dos mil doscientos treinta y cuatro")]
    public void ConvertirDecimalATexto_CaminosDeConversion_DevuelveTextoEsperado(int numero, string esperado)
    {
        Assert.Equal(esperado, NumeroATextoConverter.ConvertirDecimalATexto(numero));
    }

    [Fact]
    public void ConvertirDecimalATexto_ConFraccion_TruncaSinRedondear()
    {
        Assert.Equal("diecinueve", NumeroATextoConverter.ConvertirDecimalATexto(19.99m));
    }
}
