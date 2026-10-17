using System.Data;
using Microsoft.AspNetCore.Http;
using Npgsql;
using ProyectoArqSoft.Application.Helpers;
using ProyectoArqSoft.Infrastructure.Helpers;
using ProyectoArqSoft.Infrastructure.Persistence.Connection;
using Xunit;

namespace ProyectoArqSoft.Tests.Infrastructure;

public class PostgresDatabaseTests
{

    [Fact]
    public void NormalizeParameters_ConNull_ConservaParametroYAsignaDBNull()
    {
        using var command = new NpgsqlCommand();
        var parametro = new NpgsqlParameter { ParameterName = "valor", Value = null! };
        command.Parameters.Add(parametro);
        PostgresDatabase.NormalizeParameters(command);
        Assert.Same(parametro, Assert.Single(command.Parameters.Cast<NpgsqlParameter>()));
        Assert.Same(DBNull.Value, parametro.Value);
    }


    [Theory]
    [InlineData((sbyte)-128, (short)-128)]
    [InlineData((byte)255, (short)255)]
    public void NormalizeParameters_EnterosDeOchoBits_ConvierteAShort(object valor, short esperado)
    {
        using var command = new NpgsqlCommand();
        var parametro = command.Parameters.AddWithValue("valor", valor);
        PostgresDatabase.NormalizeParameters(command);
        Assert.Equal(esperado, Assert.IsType<short>(parametro.Value));
    }

}
