using System.Data;
using System.Data.Common;
using System.Security.Claims;
using Npgsql;

namespace ProyectoArqSoft.Infrastructure.Persistence.Connection;

// Pool compartido; contexto de auditoría local a cada transacción.
public sealed class PostgresDatabase
{
    private readonly Func<DbConnection> createConnection;
    private readonly IHttpContextAccessor httpContextAccessor;

    public PostgresDatabase(NpgsqlDataSource dataSource, IHttpContextAccessor httpContextAccessor)
        : this(dataSource.CreateConnection, httpContextAccessor) { }

    internal PostgresDatabase(Func<DbConnection> createConnection, IHttpContextAccessor httpContextAccessor)
    {
        this.createConnection = createConnection;
        this.httpContextAccessor = httpContextAccessor;
    }

    public NpgsqlConnection CreateConnection() => (NpgsqlConnection)CreateDbConnection();

    internal DbConnection CreateDbConnection() => createConnection();

    public NpgsqlTransaction BeginTransaction(NpgsqlConnection connection) =>
        (NpgsqlTransaction)BeginTransaction((DbConnection)connection);

    internal DbTransaction BeginTransaction(DbConnection connection)
    {
        var transaction = connection.BeginTransaction();
        try
        {
            var context = httpContextAccessor.HttpContext;
            var claim = context?.User.Identity?.IsAuthenticated == true
                ? context.User.FindFirstValue(ClaimTypes.NameIdentifier) : null;
            var actor = int.TryParse(claim, out var id) && id > 0 ? id.ToString() : "";
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "SELECT set_config('app.usuario_id', @actor, true), set_config('app.solicitud_id', @solicitud, true), set_config('app.motivo', @motivo, true)";
            AddParameter(command, "actor", actor);
            AddParameter(command, "solicitud", context?.TraceIdentifier ?? Guid.NewGuid().ToString("N"));
            AddParameter(command, "motivo", context?.Request.Path.Value ?? "Operacion de repositorio");
            command.ExecuteNonQuery();
            return transaction;
        }
        catch { transaction.Dispose(); throw; }
    }

    public int ExecuteNonQuery(NpgsqlCommand command) => ExecuteNonQuery((DbCommand)command);

    internal int ExecuteNonQuery(DbCommand command)
    {
        using var ownedConnection = command.Connection == null ? CreateDbConnection() : null;
        command.Connection ??= ownedConnection;
        if (command.Connection!.State != ConnectionState.Open) command.Connection.Open();
        NormalizeParameters(command);
        using var transaction = BeginTransaction(command.Connection);
        command.Transaction = transaction;
        var affected = command.ExecuteNonQuery();
        transaction.Commit();
        return affected;
    }

    public static void NormalizeParameters(NpgsqlCommand command) => NormalizeParameters((DbCommand)command);

    internal static void NormalizeParameters(DbCommand command)
    {
        foreach (DbParameter parameter in command.Parameters)
        {
            if (parameter.Value == null) parameter.Value = DBNull.Value;
            if (parameter.Value is sbyte signed) parameter.Value = (short)signed;
            if (parameter.Value is byte unsigned) parameter.Value = (short)unsigned;
        }
    }

    private static void AddParameter(DbCommand command, string name, string value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
