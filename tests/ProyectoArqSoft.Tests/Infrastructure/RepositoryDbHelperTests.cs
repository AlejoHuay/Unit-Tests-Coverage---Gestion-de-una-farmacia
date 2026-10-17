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
