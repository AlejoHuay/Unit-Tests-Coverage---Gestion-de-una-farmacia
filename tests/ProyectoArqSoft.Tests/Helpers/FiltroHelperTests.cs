using System.Data;
using Microsoft.AspNetCore.Http;
using Npgsql;
using ProyectoArqSoft.Application.Helpers;
using ProyectoArqSoft.Infrastructure.Helpers;
using ProyectoArqSoft.Infrastructure.Persistence.Connection;
using Xunit;

namespace ProyectoArqSoft.Tests.Helpers;

public class FiltroHelperTests
{

    [Fact]
    public void LimpiarFiltro_ConEspaciosMixtos_ConservaPalabrasNormalizadas()
    {
        Assert.Equal("Ácido 20", FiltroHelper.LimpiarFiltro(" \tÁcido \r\n 20  "));
    }


    [Theory]
    [InlineData(null)]
    [InlineData(" \t ")]
    public void ValidarFiltro_SinCriterio_DevuelveExito(string? filtro)
    {
        var resultado = FiltroHelper.ValidarFiltro(filtro!);
        Assert.True(resultado.IsSuccess);
        Assert.Empty(resultado.Error);
    }

}
