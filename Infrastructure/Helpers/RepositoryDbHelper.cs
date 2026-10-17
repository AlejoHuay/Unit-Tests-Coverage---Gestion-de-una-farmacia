using ProyectoArqSoft.Infrastructure.Persistence.Connection;
using System.Data.Common;
using Npgsql;

namespace ProyectoArqSoft.Infrastructure.Helpers
{
    public static class RepositoryDbHelper
    {
        public static int ExecuteNonQuery(PostgresDatabase database, NpgsqlCommand command) =>
            ExecuteNonQuery(database, (DbCommand)command);

        internal static int ExecuteNonQuery(PostgresDatabase database, DbCommand command)
        {
            using var ownedCommand = command;
            using var connection = database.CreateDbConnection();
            command.Connection = connection;
            connection.Open();
            return database.ExecuteNonQuery(command);
        }

        public static object? ExecuteScalar(PostgresDatabase database, NpgsqlCommand command) =>
            ExecuteScalar(database, (DbCommand)command);

        internal static object? ExecuteScalar(PostgresDatabase database, DbCommand command)
        {
            using var ownedCommand = command;
            using var connection = database.CreateDbConnection();
            command.Connection = connection;
            connection.Open();
            return command.ExecuteScalar();
        }

        public static T? ExecuteReaderSingle<T>(
            PostgresDatabase database,
            NpgsqlCommand command,
            Func<NpgsqlDataReader, T> mapper) =>
            ExecuteReaderSingle(database, (DbCommand)command, reader => mapper((NpgsqlDataReader)reader));

        internal static T? ExecuteReaderSingle<T>(
            PostgresDatabase database,
            DbCommand command,
            Func<DbDataReader, T> mapper)
        {
            using var ownedCommand = command;
            using var connection = database.CreateDbConnection();
            command.Connection = connection;
            connection.Open();

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return mapper(reader);
            }

            return default;
        }
    }
}
