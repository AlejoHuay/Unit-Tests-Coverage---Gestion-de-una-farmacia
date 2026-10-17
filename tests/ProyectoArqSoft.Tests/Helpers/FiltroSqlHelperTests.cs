using System.Data;
using Microsoft.AspNetCore.Http;
using Npgsql;
using ProyectoArqSoft.Application.Helpers;
using ProyectoArqSoft.Infrastructure.Helpers;
using ProyectoArqSoft.Infrastructure.Persistence.Connection;
using Xunit;

namespace ProyectoArqSoft.Tests.Helpers;

public class FiltroSqlHelperTests
{

    [Theory]
    [InlineData(" ", new[] { "nombre" })]
    [InlineData("acido", new string[0])]
    public void ConstruirCondicionLike_SinTerminosOColumnas_DevuelveVacio(string filtro, string[] columnas)
    {
        Assert.Empty(FiltroSqlHelper.ConstruirCondicionLike(filtro, columnas));
    }


    [Fact]
    public void ConstruirCondicionLike_DosTerminosYColumnas_ConstruyeGruposParametrizados()
    {
        var resultado = FiltroSqlHelper.ConstruirCondicionLike("  acido   folico  ", "m.nombre", "m.descripcion");
        const string esperado = " AND (farmacia.normalizar_texto(REPLACE(m.nombre::text, ' ', '')) LIKE farmacia.normalizar_texto(@valor0) OR farmacia.normalizar_texto(REPLACE(m.descripcion::text, ' ', '')) LIKE farmacia.normalizar_texto(@valor0))"
            + " AND (farmacia.normalizar_texto(REPLACE(m.nombre::text, ' ', '')) LIKE farmacia.normalizar_texto(@valor1) OR farmacia.normalizar_texto(REPLACE(m.descripcion::text, ' ', '')) LIKE farmacia.normalizar_texto(@valor1))";
        Assert.Equal(esperado, resultado);
    }

}
