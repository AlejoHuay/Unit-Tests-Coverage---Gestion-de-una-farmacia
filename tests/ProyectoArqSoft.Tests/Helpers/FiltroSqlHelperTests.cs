using Npgsql;
using ProyectoArqSoft.Infrastructure.Helpers;
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

    [Fact]
    public void AgregarParametrosLike_SinPalabras_NoModificaParametrosExistentes()
    {
        using var command = new NpgsqlCommand();
        var previo = command.Parameters.AddWithValue("id", 7);
        FiltroSqlHelper.AgregarParametrosLike(command, "  ");
        Assert.Same(previo, Assert.Single(command.Parameters.Cast<NpgsqlParameter>()));
        Assert.Equal(7, previo.Value);
    }

    [Fact]
    public void AgregarParametrosLike_ConComillaYTabulador_ConservaDatosEnParametros()
    {
        const string filtro = " O'Reilly  ac\tido ";
        using var command = new NpgsqlCommand("SELECT 1 WHERE true" + FiltroSqlHelper.ConstruirCondicionLike(filtro, "nombre"));
        var sql = command.CommandText;
        FiltroSqlHelper.AgregarParametrosLike(command, filtro);
        Assert.Equal(sql, command.CommandText);
        Assert.DoesNotContain("O'Reilly", sql);
        Assert.Collection(command.Parameters.Cast<NpgsqlParameter>(),
            p => { Assert.Equal("@valor0", p.ParameterName); Assert.Equal("%O'Reilly%", p.Value); },
            p => { Assert.Equal("@valor1", p.ParameterName); Assert.Equal("%acido%", p.Value); });
        Assert.Null(command.Connection);
    }
}
