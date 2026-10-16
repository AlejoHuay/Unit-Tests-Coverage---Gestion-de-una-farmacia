using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Npgsql;
using ProyectoArqSoft.Infrastructure.Persistence.Connection;
using ProyectoArqSoft.Infrastructure.Persistence.Repositories;
using Xunit;

namespace ProyectoArqSoft.Tests.Repositories;

public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VENTA_TEST_POSTGRES")))
            Skip = "Requiere tests/Invoke-PostgresTests.ps1 (PostgreSQL temporal).";
    }
}

public sealed class PostgresTheoryAttribute : TheoryAttribute
{
    public PostgresTheoryAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VENTA_TEST_POSTGRES")))
            Skip = "Requiere tests/Invoke-PostgresTests.ps1 (PostgreSQL temporal).";
    }
}

internal sealed class PostgresTestDatabase : IDisposable
{
    private readonly string _admin;
    private readonly string _name = "venta_test_" + Guid.NewGuid().ToString("N");
    private readonly NpgsqlDataSource _source;
    public VentaRepository Repository { get; }

    public PostgresTestDatabase()
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
        options.Database = _name;
        options.SearchPath = "farmacia,pg_catalog";
        options.Pooling = false;
        _source = NpgsqlDataSource.Create(options.ConnectionString);
        try
        {
            Execute(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestAssets", "postgres-schema.sql")));
            Execute("""
                INSERT INTO usuario (id,nombres,apellido_paterno,ci,ci_extencion,telefono,email,user_name,password_hash,role)
                VALUES (1,'Usuario','Prueba','123','LP','123','uno@example.test','cajero',repeat('x',60),'Admin'),
                       (2,'Editor','Prueba','456','LP','456','dos@example.test','editor',repeat('x',60),'Admin');
                INSERT INTO clasificacion(id,nombre,origen,descripcion) VALUES (1,'Analgesicos','Prueba','Prueba');
                INSERT INTO cliente(id,nit,razon_social) VALUES (1,'111','Cliente Uno'),(2,'222','Cliente Dos');
                INSERT INTO medicamento(id,nombre,presentacion,id_clasificacion,concentracion,precio,stock)
                VALUES (1,'Paracetamol','Tableta',1,'500mg',10,20),(2,'Ibuprofeno','Tableta',1,'400mg',5,30);
                """);
            var http = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "2") }, "Tests"))
            };
            Repository = new VentaRepository(new PostgresDatabase(_source, new HttpContextAccessor { HttpContext = http }));
        }
        catch { Dispose(); throw; }
    }

    public void Execute(string sql)
    {
        using var connection = _source.OpenConnection();
        using var command = new NpgsqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public T Scalar<T>(string sql)
    {
        using var connection = _source.OpenConnection();
        using var command = new NpgsqlCommand(sql, connection);
        return (T)Convert.ChangeType(command.ExecuteScalar()!, typeof(T));
    }

    public string Snapshot() => Scalar<string>("""
        SELECT jsonb_build_object(
          'ventas',(SELECT coalesce(jsonb_agg(to_jsonb(v) ORDER BY id),'[]') FROM venta v),
          'detalles',(SELECT coalesce(jsonb_agg(to_jsonb(d) ORDER BY id_venta,id_medicamento),'[]') FROM detalle_venta d),
          'medicamentos',(SELECT jsonb_agg(to_jsonb(m) ORDER BY id) FROM medicamento m),
          'auditoria',(SELECT count(*) FROM auditoria))::text
        """);

    // Inyeccion de fallo solo en la base unica de este caso, sin alterar el esquema del proyecto.
    public void FailVentaUpdate(bool exception) => Execute(exception ? """
        CREATE FUNCTION test_fail() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN RAISE EXCEPTION 'fallo controlado'; END $$;
        CREATE TRIGGER test_fail BEFORE UPDATE ON venta FOR EACH ROW EXECUTE FUNCTION test_fail();
        """ : """
        CREATE FUNCTION test_fail() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN RETURN NULL; END $$;
        CREATE TRIGGER test_fail BEFORE UPDATE ON venta FOR EACH ROW EXECUTE FUNCTION test_fail();
        """);

    public void Dispose()
    {
        _source.Dispose();
        // El nombre se genera internamente, nunca se obtiene de configuracion ni de datos del usuario.
        using var connection = new NpgsqlConnection(_admin);
        connection.Open();
        using var command = new NpgsqlCommand($"DROP DATABASE {_name} WITH (FORCE)", connection);
        command.ExecuteNonQuery();
    }
}
