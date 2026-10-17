using System.Data;
using System.Data.Common;
using Moq;
using Moq.Protected;
using Npgsql;
using ProyectoArqSoft.Infrastructure.Helpers;
using Xunit;

namespace ProyectoArqSoft.Tests.Infrastructure;

public class RepositoryDbHelperTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExecuteReaderSingle_FallaLecturaOMapeador_PropagaErrorYLiberaLosTresRecursos(bool fallaMapeador)
    {
        var mocks = new DatabaseMocks();
        var command = DatabaseMocks.Command();
        var reader = new Mock<DbDataReader>(MockBehavior.Strict);
        var mapper = new Mock<Func<DbDataReader, string>>(MockBehavior.Strict);
        var error = new InvalidOperationException("lectura o mapeo fallido");
        reader.Protected().Setup("Dispose", ItExpr.IsAny<bool>());
        command.Protected().Setup<DbDataReader>("ExecuteDbDataReader", ItExpr.IsAny<CommandBehavior>()).Returns(reader.Object);
        if (fallaMapeador)
        {
            reader.Setup(r => r.Read()).Returns(true);
            mapper.Setup(m => m(reader.Object)).Throws(error);
        }
        else reader.Setup(r => r.Read()).Throws(error);

        Assert.Same(error, Assert.Throws<InvalidOperationException>(() =>
            RepositoryDbHelper.ExecuteReaderSingle(mocks.Database, command.Object, mapper.Object)));

        reader.Verify(r => r.Read(), Times.Once);
        mapper.Verify(m => m(reader.Object), fallaMapeador ? Times.Once() : Times.Never());
        mapper.VerifyNoOtherCalls();
        reader.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
        command.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
        mocks.Connection.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExecuteReaderSingle_ConYSinFilas_MapeaSoloLaPrimeraOLaOmite(bool hayFilas)
    {
        var mocks = new DatabaseMocks();
        var command = DatabaseMocks.Command();
        var reader = new Mock<DbDataReader>(MockBehavior.Strict);
        reader.SetupSequence(r => r.Read()).Returns(hayFilas).Returns(true).Returns(false);
        reader.Setup(r => r.GetInt32(0)).Returns(7);
        reader.Protected().Setup("Dispose", ItExpr.IsAny<bool>());
        command.Protected().Setup<DbDataReader>("ExecuteDbDataReader", ItExpr.IsAny<CommandBehavior>()).Returns(reader.Object);
        var mapper = new Mock<Func<DbDataReader, string>>(MockBehavior.Strict);
        if (hayFilas) mapper.Setup(m => m(reader.Object)).Returns((DbDataReader r) => "fila-" + r.GetInt32(0));

        var resultado = RepositoryDbHelper.ExecuteReaderSingle(mocks.Database, command.Object, mapper.Object);

        Assert.Equal(hayFilas ? "fila-7" : null, resultado);
        mapper.Verify(m => m(reader.Object), hayFilas ? Times.Once() : Times.Never());
        mapper.VerifyNoOtherCalls();
        reader.Verify(r => r.Read(), Times.Once);
        reader.Verify(r => r.GetInt32(0), hayFilas ? Times.Once() : Times.Never());
        command.Protected().Verify("ExecuteDbDataReader", Times.Once(), CommandBehavior.Default);
        reader.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
        command.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
        mocks.Connection.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
        mocks.Connection.Protected().Verify("BeginDbTransaction", Times.Never(), ItExpr.IsAny<IsolationLevel>());
        mocks.Factory.Verify(f => f(), Times.Once);
        mocks.Connection.Verify(c => c.Open(), Times.Once);
    }

    [Theory]
    [InlineData("valor")]
    [InlineData("null-sql")]
    [InlineData("sin-filas")]
    public void ExecuteScalar_ConDistintosResultados_DevuelveElOriginalSinIniciarTransaccion(string caso)
    {
        var mocks = new DatabaseMocks();
        var command = DatabaseMocks.Command();
        object? esperado = caso switch { "valor" => 17.5m, "null-sql" => DBNull.Value, _ => null };
        command.Setup(c => c.ExecuteScalar()).Returns(esperado);

        Assert.Same(esperado, RepositoryDbHelper.ExecuteScalar(mocks.Database, command.Object));

        Assert.Same(mocks.Connection.Object, command.Object.Connection);
        command.Verify(c => c.ExecuteScalar(), Times.Once);
        mocks.Factory.Verify(f => f(), Times.Once);
        mocks.Connection.Verify(c => c.Open(), Times.Once);
        mocks.Connection.Protected().Verify("BeginDbTransaction", Times.Never(), ItExpr.IsAny<IsolationLevel>());
        mocks.Http.VerifyGet(h => h.HttpContext, Times.Never);
        command.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
        mocks.Connection.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
    }

    [Fact]
    public void ExecuteNonQuery_ConFilasAfectadas_ConfirmaYLiberaComandoYConexion()
    {
        var mocks = new DatabaseMocks();
        var command = DatabaseMocks.Command();
        command.Setup(c => c.ExecuteNonQuery()).Returns(2);

        Assert.Equal(2, RepositoryDbHelper.ExecuteNonQuery(mocks.Database, command.Object));

        Assert.Same(mocks.Connection.Object, command.Object.Connection);
        Assert.Same(mocks.Transaction.Object, command.Object.Transaction);
        mocks.Factory.Verify(f => f(), Times.Once);
        mocks.Connection.Verify(c => c.Open(), Times.Once);
        mocks.Transaction.Verify(t => t.Commit(), Times.Once);
        command.Verify(c => c.ExecuteNonQuery(), Times.Once);
        command.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
        mocks.Connection.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
        Assert.Equal(ConnectionState.Closed, mocks.State);
    }
}
