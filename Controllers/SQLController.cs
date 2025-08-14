using Npgsql;

namespace FenDB.Bot;

public class SQLController
{
    public static NpgsqlConnection connection;

    public async Task Initialize()
    {
        var host = "localhost";
        var port = 5432;
        var username = "fendb";
        var dbName = "fendb";

        try
        {
            connection = new NpgsqlConnection($"Host={host};Port={port};Database=postgres;Username={username};Password=;");
            connection.Open();

            using (var command = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @dbname", connection))
            {
                command.Parameters.AddWithValue("dbname", dbName);
                var exists = command.ExecuteScalar();
                if (exists != null)
                {
                    Console.WriteLine($"DB {dbName} exist, connecting to DB...");
                }
                else
                {
                    using var createCommand = new NpgsqlCommand($"CREATE DATABASE \"{dbName}\"", connection);
                    createCommand.ExecuteNonQuery();
                    Console.WriteLine($"DB {dbName} was created, connecting to DB...");
                }
            }
            connection.Close();

            connection = new NpgsqlConnection($"Host={host};Port={port};Database=fendb;Username={username};Password=;");
            connection.Open();


            /*Initialize DB*/
            /*Warns*/
            string createWarnTableSql = @"
            CREATE TABLE IF NOT EXISTS warns (
                id BIGSERIAL PRIMARY KEY,
                server_id VARCHAR(20) NOT NULL,
                user_id VARCHAR(20) NOT NULL,
                warn_date TIMESTAMP NOT NULL DEFAULT NOW(),
                expire_date TIMESTAMP,
                reason TEXT
            );
            CREATE INDEX IF NOT EXISTS idx_warns_server_user
                ON warns(server_id, user_id);
            ";

            using var createWarnsTable = new NpgsqlCommand(createWarnTableSql, connection);
            createWarnsTable.ExecuteNonQuery();


            /*serverSettings*/
            string createServerSettingsTableSql = @"
            CREATE TABLE IF NOT EXISTS serverSettings (
                server_id VARCHAR(20) PRIMARY KEY,
                channelLog_id VARCHAR(20) NULL,
                warnCountBeforeBan SMALLINT NULL,
                banrole_id VARCHAR(20) NULL
            );
            ";

            using var createServerSettingsTable = new NpgsqlCommand(createServerSettingsTableSql, connection);
            createServerSettingsTable.ExecuteNonQuery();


            Console.WriteLine($"Connected to DB {dbName}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public static object? ExecuteQuery(string Query)
    {
        using var QuerryCommand = new NpgsqlCommand(Query, connection);
        return QuerryCommand.ExecuteScalar();
    }
    public static void ExecuteUpdate(string Query)
    {
        using var QuerryCommand = new NpgsqlCommand(Query, connection);
        QuerryCommand.ExecuteNonQuery();
    }
}