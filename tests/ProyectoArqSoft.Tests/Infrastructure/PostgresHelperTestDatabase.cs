using Microsoft.AspNetCore.Http;
using Npgsql;
using ProyectoArqSoft.Infrastructure.Persistence.Connection;

namespace ProyectoArqSoft.Tests.Infrastructure;

// Solo para los casos PostgresIntegration; cada caso dispone de una base aislada.
internal sealed class PostgresHelperTestDatabase : IDisposable
{
    private readonly string _admin;
    private readonly string _name = "helpers_test_" + Guid.NewGuid().ToString("N");
    private NpgsqlDataSource? _source;
    public NpgsqlDataSource Source => _source!;

    public PostgresHelperTestDatabase()
    {
        _admin = Environment.GetEnvironmentVariable("VENTA_TEST_POSTGRES")
            ?? throw new InvalidOperationException("Ejecuta tests/Invoke-PostgresTests.ps1.");
        var options = new NpgsqlConnectionStringBuilder(_admin);
        if (options.Host != "127.0.0.1" || options.Username != "venta_test" || options.Database != "postgres")
            throw new InvalidOperationException("Solo se admite la instancia local exclusiva de pruebas.");
        using (var connection = new NpgsqlConnection(_admin))
        {
            connection.Open();
            using var command = new NpgsqlCommand($"CREATE DATABASE {_name} TEMPLATE template0", connection);
            command.ExecuteNonQuery();
        }
        try
        {
            options.Database = _name;
            options.Pooling = false;
            _source = NpgsqlDataSource.Create(options.ConnectionString);
            using var connection = Source.OpenConnection();
            using var command = new NpgsqlCommand("CREATE TABLE prueba (id integer PRIMARY KEY, valor text)", connection);
            command.ExecuteNonQuery();
        }
        catch { Dispose(); throw; }
    }

    public PostgresDatabase Database(IHttpContextAccessor? accessor = null) =>
        new(Source, accessor ?? new HttpContextAccessor());

    public object? Scalar(string sql)
    {
        using var connection = Source.OpenConnection();
        using var command = new NpgsqlCommand(sql, connection);
        return command.ExecuteScalar();
    }

    public void Dispose()
    {
        _source?.Dispose();
        using var connection = new NpgsqlConnection(_admin);
        connection.Open();
        // Identificador generado internamente; nunca se toma del entorno ni del usuario.
        using var command = new NpgsqlCommand($"DROP DATABASE {_name} WITH (FORCE)", connection);
        command.ExecuteNonQuery();
    }
}
