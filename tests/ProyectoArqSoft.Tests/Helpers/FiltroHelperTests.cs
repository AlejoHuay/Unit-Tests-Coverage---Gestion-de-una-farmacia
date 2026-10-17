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


    [Theory]
    [InlineData(2, "El criterio debe tener al menos 3 caracteres.")]
    [InlineData(3, "")]
    [InlineData(45, "")]
    [InlineData(46, "El criterio no puede tener más de 45 caracteres.")]
    public void ValidarFiltro_LimitesPredeterminados_MideDespuesDeLimpiar(int longitud, string error)
    {
        var resultado = FiltroHelper.ValidarFiltro("  " + new string('a', longitud) + "  ");
        Assert.Equal(error.Length == 0, resultado.IsSuccess);
        Assert.Equal(error, resultado.Error);
    }


    [Theory]
    [InlineData("a", "El criterio debe tener al menos 2 caracteres.")]
    [InlineData("abcde", "El criterio no puede tener más de 4 caracteres.")]
    [InlineData("ab", "")]
    public void ValidarFiltro_LimitesPersonalizados_UsaLosArgumentos(string filtro, string error)
    {
        var resultado = FiltroHelper.ValidarFiltro(filtro, 2, 4);
        Assert.Equal(error.Length == 0, resultado.IsSuccess);
        Assert.Equal(error, resultado.Error);
    }


    [Theory]
    [InlineData("Ácido 20 - forte", true)]
    [InlineData("Ácido+20", false)]
    public void ValidarFiltro_CaracteresPermitidos_DevuelveResultadoCorrespondiente(string filtro, bool valido)
    {
        var resultado = FiltroHelper.ValidarFiltro(filtro);
        Assert.Equal(valido, resultado.IsSuccess);
        Assert.Equal(valido ? "" : "Criterio inválido.", resultado.Error);
    }

}
