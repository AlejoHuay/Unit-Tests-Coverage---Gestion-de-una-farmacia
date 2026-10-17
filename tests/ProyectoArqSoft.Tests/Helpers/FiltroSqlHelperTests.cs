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

}
