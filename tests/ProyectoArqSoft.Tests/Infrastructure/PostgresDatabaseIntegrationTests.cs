using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using Npgsql;
using ProyectoArqSoft.Infrastructure.Helpers;
using ProyectoArqSoft.Infrastructure.Persistence.Connection;
using ProyectoArqSoft.Tests.Repositories;
using Xunit;

namespace ProyectoArqSoft.Tests.Infrastructure;

[Trait("Category", "PostgresIntegration")]
public class PostgresDatabaseIntegrationTests
{

    [PostgresTheory]
    [InlineData("7", true, "7")]
    [InlineData("7", false, "")]
    [InlineData("abc", true, "")]
    [InlineData("0", true, "")]
    [InlineData(null, true, "")]
    public void BeginTransaction_ConContexto_AsignaAuditoriaLocalSinFiltrarlaTrasCommit(string? claim, bool autenticado, string actor)
    {
        using var db = new PostgresHelperTestDatabase();
        var claims = claim == null ? Array.Empty<Claim>() : new[] { new Claim(ClaimTypes.NameIdentifier, claim) };
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, autenticado ? "Tests" : null)),
            TraceIdentifier = "solicitud-'prueba'"
        };
        context.Request.Path = "/prueba/actualizar";
        var accessor = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
        accessor.SetupGet(a => a.HttpContext).Returns(context);
        using var connection = db.Source.OpenConnection();
        using var transaction = db.Database(accessor.Object).BeginTransaction(connection);
        using (var command = new NpgsqlCommand("SELECT current_setting('app.usuario_id'), current_setting('app.solicitud_id'), current_setting('app.motivo')", connection, transaction))
        using (var reader = command.ExecuteReader())
        {
            Assert.True(reader.Read());
            Assert.Equal(actor, reader.GetString(0));
            Assert.Equal(context.TraceIdentifier, reader.GetString(1));
            Assert.Equal(context.Request.Path.Value, reader.GetString(2));
        }
        transaction.Commit();
        using var after = new NpgsqlCommand("SELECT current_setting('app.usuario_id') || current_setting('app.solicitud_id') || current_setting('app.motivo')", connection);
        Assert.Equal("", after.ExecuteScalar());
        accessor.VerifyGet(a => a.HttpContext, Times.Once);
        accessor.VerifyNoOtherCalls();
    }


    [PostgresFact]
    public void BeginTransaction_SinContexto_GeneraSolicitudYMotivoDeRespaldo()
    {
        using var db = new PostgresHelperTestDatabase();
        var accessor = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
        accessor.SetupGet(a => a.HttpContext).Returns((HttpContext?)null);
        using var connection = db.Source.OpenConnection();
        using var transaction = db.Database(accessor.Object).BeginTransaction(connection);
        using var command = new NpgsqlCommand("SELECT current_setting('app.usuario_id'), current_setting('app.solicitud_id'), current_setting('app.motivo')", connection, transaction);
        using var reader = command.ExecuteReader();
        Assert.True(reader.Read());
        Assert.Empty(reader.GetString(0));
        Assert.True(Guid.TryParseExact(reader.GetString(1), "N", out var requestId));
        Assert.NotEqual(Guid.Empty, requestId);
        Assert.Equal("Operacion de repositorio", reader.GetString(2));
        accessor.VerifyGet(a => a.HttpContext, Times.Once);
        accessor.VerifyNoOtherCalls();
    }


    [PostgresFact]
    public void BeginTransaction_SinIdentidad_UsaActorVacio()
    {
        using var db = new PostgresHelperTestDatabase();
        var context = new DefaultHttpContext { User = new ClaimsPrincipal() };
        var accessor = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
        accessor.SetupGet(a => a.HttpContext).Returns(context);
        using var connection = db.Source.OpenConnection();
        using var transaction = db.Database(accessor.Object).BeginTransaction(connection);
        using var command = new NpgsqlCommand("SELECT current_setting('app.usuario_id')", connection, transaction);
        Assert.Equal("", command.ExecuteScalar());
        accessor.VerifyGet(a => a.HttpContext, Times.Once);
        accessor.VerifyNoOtherCalls();
    }


    [PostgresFact]
    public void BeginTransaction_FalloDelContexto_PropagaExcepcionYPermiteOtraTransaccion()
    {
        using var db = new PostgresHelperTestDatabase();
        var fallo = new InvalidOperationException("fallo controlado del contexto");
        var accessor = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
        accessor.SetupGet(a => a.HttpContext).Throws(fallo);
        using var connection = db.Source.OpenConnection();
        Assert.Same(fallo, Assert.Throws<InvalidOperationException>(() => db.Database(accessor.Object).BeginTransaction(connection)));
        Assert.Equal(ConnectionState.Open, connection.State);
        using var nueva = connection.BeginTransaction();
        using var command = new NpgsqlCommand("SELECT 1", connection, nueva);
        Assert.Equal(1, command.ExecuteScalar());
        nueva.Commit();
        accessor.VerifyGet(a => a.HttpContext, Times.Once);
        accessor.VerifyNoOtherCalls();
    }


    [PostgresTheory]
    [InlineData("propia")]
    [InlineData("prestada-cerrada")]
    [InlineData("prestada-abierta")]
    public void ExecuteNonQuery_PropiedadYEstadoDeConexion_ConfirmaYNormaliza(string modo)
    {
        using var db = new PostgresHelperTestDatabase();
        using var prestada = modo == "propia" ? null : db.Source.CreateConnection();
        if (modo == "prestada-abierta") prestada!.Open();
        using var command = new NpgsqlCommand("INSERT INTO prueba(id, valor) VALUES (@id, @valor)", prestada);
        command.Parameters.AddWithValue("id", (sbyte)7);
        command.Parameters.Add(new NpgsqlParameter { ParameterName = "valor", Value = null! });
        var resultado = db.Database().ExecuteNonQuery(command);
        Assert.Equal(1, resultado);
        Assert.Equal((short)7, Assert.IsType<short>(command.Parameters[0].Value));
        Assert.Same(DBNull.Value, command.Parameters[1].Value);
        Assert.Equal(1L, db.Scalar("SELECT count(*) FROM prueba WHERE id=7 AND valor IS NULL"));
        if (prestada != null)
        {
            Assert.Same(prestada, command.Connection);
            Assert.Equal(ConnectionState.Open, prestada.State);
            using var siguiente = prestada.BeginTransaction();
            siguiente.Rollback();
        }
        else
        {
            Assert.Equal(ConnectionState.Closed, command.Connection!.State);
        }
    }

}
