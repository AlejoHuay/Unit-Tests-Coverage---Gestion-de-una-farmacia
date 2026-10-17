using System.Data;
using Microsoft.AspNetCore.Http;
using Npgsql;
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

    [Fact]
    public void NormalizeParameters_ValoresCompatibles_ConservaTiposValoresYMetadatos()
    {
        using var command = new NpgsqlCommand("SELECT @texto");
        object[] valores = { DBNull.Value, 17, 12.5m, "Ácido" };
        foreach (var valor in valores)
            command.Parameters.AddWithValue("p" + command.Parameters.Count, valor);
        var parametros = command.Parameters.Cast<NpgsqlParameter>().ToArray();
        PostgresDatabase.NormalizeParameters(command);
        Assert.Equal("SELECT @texto", command.CommandText);
        Assert.Equal(valores.Length, command.Parameters.Count);
        for (var i = 0; i < valores.Length; i++)
        {
            Assert.Same(parametros[i], command.Parameters[i]);
            Assert.Same(valores[i], command.Parameters[i].Value);
            Assert.Equal("p" + i, command.Parameters[i].ParameterName);
        }
    }

    [Fact]
    public void CreateConnection_ConDataSource_DevuelveConexionesIndependientesCerradas()
    {
        using var source = NpgsqlDataSource.Create("Host=127.0.0.1;Port=1;Database=prueba;Username=prueba;Pooling=false");
        var database = new PostgresDatabase(source, new HttpContextAccessor());
        using var primera = database.CreateConnection();
        using var segunda = database.CreateConnection();
        Assert.NotSame(primera, segunda);
        Assert.Equal(ConnectionState.Closed, primera.State);
        Assert.Equal(ConnectionState.Closed, segunda.State);
        Assert.Equal("prueba", primera.Database);
    }
}
