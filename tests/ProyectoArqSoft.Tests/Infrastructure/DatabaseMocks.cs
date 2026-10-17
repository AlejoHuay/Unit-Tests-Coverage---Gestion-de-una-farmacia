using System.Collections;
using System.Data;
using System.Data.Common;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using Npgsql;
using ProyectoArqSoft.Infrastructure.Persistence.Connection;

namespace ProyectoArqSoft.Tests.Infrastructure;

// Dobles ADO.NET en memoria: ninguna operacion abre una conexion real.
internal sealed class DatabaseMocks
{
    public Mock<DbConnection> Connection { get; } = new(MockBehavior.Strict);
    public Mock<DbTransaction> Transaction { get; } = new(MockBehavior.Strict);
    public Mock<DbCommand> Audit { get; } = Command();
    public Mock<Func<DbConnection>> Factory { get; } = new(MockBehavior.Strict);
    public Mock<IHttpContextAccessor> Http { get; } = new(MockBehavior.Strict);
    public List<string> Events { get; } = new();
    public ConnectionState State { get; set; } = ConnectionState.Closed;
    public PostgresDatabase Database { get; }

    public DatabaseMocks(HttpContext? context = null)
    {
        Http.SetupGet(h => h.HttpContext).Returns(context);
        Factory.Setup(f => f()).Returns(Connection.Object);
        Connection.SetupGet(c => c.State).Returns(() => State);
        Connection.Setup(c => c.Open()).Callback(() => { State = ConnectionState.Open; Events.Add("abrir"); });
        Connection.Protected().Setup<DbTransaction>("BeginDbTransaction", ItExpr.IsAny<IsolationLevel>())
            .Callback(() => Events.Add("iniciar")).Returns(Transaction.Object);
        Connection.Protected().Setup<DbCommand>("CreateDbCommand").Returns(Audit.Object);
        Connection.Protected().Setup("Dispose", ItExpr.IsAny<bool>())
            .Callback(() => { State = ConnectionState.Closed; Events.Add("cerrar"); });
        Audit.Setup(c => c.ExecuteNonQuery()).Callback(() => Events.Add("auditar")).Returns(1);
        Transaction.Setup(t => t.Commit()).Callback(() => Events.Add("confirmar"));
        Transaction.Protected().Setup("Dispose", ItExpr.IsAny<bool>()).Callback(() => Events.Add("liberar-transaccion"));
        Database = new PostgresDatabase(Factory.Object, Http.Object);
    }

    public static Mock<DbCommand> Command()
    {
        var command = new Mock<DbCommand>(MockBehavior.Strict);
        command.SetupAllProperties();
        var values = new List<DbParameter>();
        var parameters = new Mock<DbParameterCollection>(MockBehavior.Strict);
        parameters.SetupGet(p => p.Count).Returns(() => values.Count);
        parameters.Setup(p => p.Add(It.IsAny<object>())).Returns((object value) =>
        {
            values.Add((DbParameter)value);
            return values.Count - 1;
        });
        parameters.Setup(p => p.GetEnumerator()).Returns(() => ((IEnumerable)values).GetEnumerator());
        parameters.Protected().Setup<DbParameter>("GetParameter", ItExpr.IsAny<int>()).Returns((int index) => values[index]);
        parameters.Protected().Setup<DbParameter>("GetParameter", ItExpr.IsAny<string>()).Returns((string name) => values.Single(p => p.ParameterName == name));
        command.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(parameters.Object);
        command.Protected().Setup<DbParameter>("CreateDbParameter").Returns(() => new NpgsqlParameter());
        command.Protected().Setup("Dispose", ItExpr.IsAny<bool>());
        return command;
    }
}
