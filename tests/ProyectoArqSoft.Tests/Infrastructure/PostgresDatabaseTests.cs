using System.Data;
using System.Data.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using Npgsql;
using ProyectoArqSoft.Infrastructure.Persistence.Connection;
using Xunit;

namespace ProyectoArqSoft.Tests.Infrastructure;

public class PostgresDatabaseTests
{
    [Fact]
    public void ExecuteNonQuery_FallaCommit_PropagaErrorYLiberaRecursosSinReintentar()
    {
        var mocks = new DatabaseMocks();
        var command = DatabaseMocks.Command();
        command.Setup(c => c.ExecuteNonQuery()).Returns(1);
        var error = new InvalidOperationException("confirmacion fallida");
        mocks.Transaction.Setup(t => t.Commit()).Throws(error);

        Assert.Same(error, Assert.Throws<InvalidOperationException>(() => mocks.Database.ExecuteNonQuery(command.Object)));

        command.Verify(c => c.ExecuteNonQuery(), Times.Once);
        mocks.Transaction.Verify(t => t.Commit(), Times.Once);
        mocks.Transaction.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
        mocks.Connection.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
        command.Protected().Verify("Dispose", Times.Never(), ItExpr.IsAny<bool>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExecuteNonQuery_FallaEscritura_NoConfirmaYLiberaSoloRecursosPropios(bool conexionPrestada)
    {
        var mocks = new DatabaseMocks();
        var command = DatabaseMocks.Command();
        if (conexionPrestada) command.Object.Connection = mocks.Connection.Object;
        var error = new InvalidOperationException("escritura rechazada");
        command.Setup(c => c.ExecuteNonQuery()).Throws(error);

        Assert.Same(error, Assert.Throws<InvalidOperationException>(() => mocks.Database.ExecuteNonQuery(command.Object)));

        command.Verify(c => c.ExecuteNonQuery(), Times.Once);
        mocks.Audit.Verify(c => c.ExecuteNonQuery(), Times.Once);
        mocks.Transaction.Verify(t => t.Commit(), Times.Never);
        mocks.Transaction.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
        mocks.Connection.Protected().Verify("Dispose", conexionPrestada ? Times.Never() : Times.Once(), ItExpr.IsAny<bool>());
        command.Protected().Verify("Dispose", Times.Never(), ItExpr.IsAny<bool>());
        Assert.Equal(conexionPrestada ? ConnectionState.Open : ConnectionState.Closed, mocks.State);
    }

    [Theory]
    [InlineData("propia")]
    [InlineData("prestada-cerrada")]
    [InlineData("prestada-abierta")]
    public void ExecuteNonQuery_PropiedadYEstadoDeConexion_NormalizaAuditaYConfirma(string modo)
    {
        var mocks = new DatabaseMocks();
        var command = DatabaseMocks.Command();
        if (modo != "propia") command.Object.Connection = mocks.Connection.Object;
        if (modo == "prestada-abierta") mocks.State = ConnectionState.Open;
        command.Object.Parameters.Add(new NpgsqlParameter("id", (byte)255));
        command.Object.Parameters.Add(new NpgsqlParameter { ParameterName = "valor", Value = null! });
        command.Setup(c => c.ExecuteNonQuery()).Callback(() => mocks.Events.Add("escribir")).Returns(2);

        Assert.Equal(2, mocks.Database.ExecuteNonQuery(command.Object));

        Assert.Equal((short)255, Assert.IsType<short>(command.Object.Parameters[0].Value));
        Assert.Same(DBNull.Value, command.Object.Parameters[1].Value);
        Assert.Same(mocks.Connection.Object, command.Object.Connection);
        Assert.Same(mocks.Transaction.Object, command.Object.Transaction);
        var esperado = new List<string>();
        if (modo != "prestada-abierta") esperado.Add("abrir");
        esperado.AddRange(new[] { "iniciar", "auditar", "escribir", "confirmar", "liberar-transaccion" });
        if (modo == "propia") esperado.Add("cerrar");
        Assert.Equal(esperado, mocks.Events);
        mocks.Factory.Verify(f => f(), modo == "propia" ? Times.Once() : Times.Never());
        command.Verify(c => c.ExecuteNonQuery(), Times.Once);
        command.Protected().Verify("Dispose", Times.Never(), ItExpr.IsAny<bool>());
        Assert.Equal(modo == "propia" ? ConnectionState.Closed : ConnectionState.Open, mocks.State);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BeginTransaction_FallaAuditoria_LiberaTransaccionYPropagaMismoError(bool fallaComando)
    {
        var mocks = new DatabaseMocks();
        var error = new InvalidOperationException("fallo de auditoria");
        if (fallaComando) mocks.Audit.Setup(c => c.ExecuteNonQuery()).Throws(error);
        else mocks.Http.SetupGet(h => h.HttpContext).Throws(error);

        Assert.Same(error, Assert.Throws<InvalidOperationException>(() => mocks.Database.BeginTransaction(mocks.Connection.Object)));

        mocks.Transaction.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
        mocks.Transaction.Verify(t => t.Commit(), Times.Never);
        mocks.Connection.Protected().Verify("Dispose", Times.Never(), ItExpr.IsAny<bool>());
        mocks.Audit.Protected().Verify("Dispose", fallaComando ? Times.Once() : Times.Never(), ItExpr.IsAny<bool>());
        mocks.Audit.Verify(c => c.ExecuteNonQuery(), fallaComando ? Times.Once() : Times.Never());
        mocks.Http.VerifyGet(h => h.HttpContext, Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BeginTransaction_SinUsuario_ConservaContextoOUsaRespaldo(bool hayContexto)
    {
        var context = hayContexto ? new DefaultHttpContext
        {
            User = new ClaimsPrincipal(),
            TraceIdentifier = "solicitud-sin-usuario"
        } : null;
        if (context != null) context.Request.Path = "/sin-usuario";
        var mocks = new DatabaseMocks(context);

        Assert.Same(mocks.Transaction.Object, mocks.Database.BeginTransaction(mocks.Connection.Object));

        Assert.Equal("", mocks.Audit.Object.Parameters["actor"].Value);
        var solicitud = Assert.IsType<string>(mocks.Audit.Object.Parameters["solicitud"].Value);
        if (hayContexto) Assert.Equal("solicitud-sin-usuario", solicitud);
        else
        {
            Assert.True(Guid.TryParseExact(solicitud, "N", out var id));
            Assert.NotEqual(Guid.Empty, id);
        }
        Assert.Equal(hayContexto ? "/sin-usuario" : "Operacion de repositorio", mocks.Audit.Object.Parameters["motivo"].Value);
        mocks.Audit.Verify(c => c.ExecuteNonQuery(), Times.Once);
        mocks.Audit.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
        mocks.Http.VerifyGet(h => h.HttpContext, Times.Once);
    }

    [Theory]
    [InlineData("7", true, "7")]
    [InlineData("7", false, "")]
    [InlineData("invalido", true, "")]
    [InlineData("0", true, "")]
    [InlineData(null, true, "")]
    public void BeginTransaction_ConIdentidad_ConfiguraAuditoriaSinConfirmarLaTransaccion(string? id, bool autenticado, string actor)
    {
        var claims = id == null ? Array.Empty<Claim>() : new[] { new Claim(ClaimTypes.NameIdentifier, id) };
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, autenticado ? "Tests" : null)),
            TraceIdentifier = "solicitud-'prueba'"
        };
        context.Request.Path = "/clasificaciones/editar";
        var mocks = new DatabaseMocks(context);

        var resultado = mocks.Database.BeginTransaction(mocks.Connection.Object);

        Assert.Same(mocks.Transaction.Object, resultado);
        Assert.Same(resultado, mocks.Audit.Object.Transaction);
        Assert.Equal("SELECT set_config('app.usuario_id', @actor, true), set_config('app.solicitud_id', @solicitud, true), set_config('app.motivo', @motivo, true)", mocks.Audit.Object.CommandText);
        Assert.Collection(mocks.Audit.Object.Parameters.Cast<DbParameter>(),
            p => { Assert.Equal("actor", p.ParameterName); Assert.Equal(actor, p.Value); },
            p => { Assert.Equal("solicitud", p.ParameterName); Assert.Equal(context.TraceIdentifier, p.Value); },
            p => { Assert.Equal("motivo", p.ParameterName); Assert.Equal(context.Request.Path.Value, p.Value); });
        mocks.Audit.Verify(c => c.ExecuteNonQuery(), Times.Once);
        mocks.Audit.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
        mocks.Transaction.Verify(t => t.Commit(), Times.Never);
        mocks.Transaction.Protected().Verify("Dispose", Times.Never(), ItExpr.IsAny<bool>());
        mocks.Http.VerifyGet(h => h.HttpContext, Times.Once);
        mocks.Http.VerifyNoOtherCalls();
        mocks.Factory.Verify(f => f(), Times.Never);
    }

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
