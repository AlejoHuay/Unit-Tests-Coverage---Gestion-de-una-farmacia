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
public class RepositoryDbHelperTests
{

    [PostgresFact]
    public void ExecuteNonQuery_ConParametros_DevuelveFilasConfirmaAuditoriaYLiberaRecursos()
    {
        using var db = new PostgresHelperTestDatabase();
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "7") }, "Tests"))
        };
        var accessor = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
        accessor.SetupGet(a => a.HttpContext).Returns(context);
        using var command = new NpgsqlCommand("INSERT INTO prueba(id,valor) VALUES (@id,current_setting('app.usuario_id'))");
        command.Parameters.AddWithValue("id", (byte)7);
        Assert.Equal(1, RepositoryDbHelper.ExecuteNonQuery(db.Database(accessor.Object), command));
        Assert.Equal("7", db.Scalar("SELECT valor FROM prueba WHERE id=7"));
        Assert.Throws<ObjectDisposedException>(() => command.ExecuteScalar());
        Assert.Equal(ConnectionState.Closed, command.Connection!.State);
        accessor.VerifyGet(a => a.HttpContext, Times.Once);
        accessor.VerifyNoOtherCalls();
    }


    [PostgresTheory]
    [InlineData("SELECT 7", "valor")]
    [InlineData("SELECT NULL::integer", "dbnull")]
    [InlineData("SELECT 7 WHERE false", "sin-filas")]
    public void ExecuteScalar_ResultadoDelServidor_ConservaValorYLiberaRecursos(string sql, string caso)
    {
        using var db = new PostgresHelperTestDatabase();
        using var command = new NpgsqlCommand(sql);
        var resultado = RepositoryDbHelper.ExecuteScalar(db.Database(), command);
        if (caso == "valor") Assert.Equal(7, resultado);
        else if (caso == "dbnull") Assert.Same(DBNull.Value, resultado);
        else Assert.Null(resultado);
        Assert.Throws<ObjectDisposedException>(() => command.ExecuteScalar());
        Assert.Equal(ConnectionState.Closed, command.Connection!.State);
    }

}
