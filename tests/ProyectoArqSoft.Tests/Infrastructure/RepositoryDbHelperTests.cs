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
