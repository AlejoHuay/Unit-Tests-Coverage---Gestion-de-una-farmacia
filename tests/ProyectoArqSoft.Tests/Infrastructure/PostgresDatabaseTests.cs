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

}
