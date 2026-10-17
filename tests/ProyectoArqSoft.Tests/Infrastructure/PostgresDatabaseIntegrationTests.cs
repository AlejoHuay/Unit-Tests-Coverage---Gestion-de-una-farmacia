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

}
