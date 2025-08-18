using Npgsql;
using PulsNet.Services;
using System.Data;

namespace PulsNet.Data
{
    public sealed class Db
    {
        private readonly string _connectionString;

        public Db(ConfigService config)
        {
            var c = config.Config.Database;
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = c.Host,
                Port = c.Port,
                Database = c.Database,
                Username = c.Username,
                Password = c.Password,
                SslMode = SslMode.Disable
            };
            _connectionString = builder.ToString();
        }

        public async Task<int> ExecuteAsync(string sql, object? parameters = null, CancellationToken ct = default)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(sql, conn);
            AddParameters(cmd, parameters);
            return await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task<T?> QuerySingleAsync<T>(string sql, Func<IDataReader, T> map, object? parameters = null, CancellationToken ct = default)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(sql, conn);
            AddParameters(cmd, parameters);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return map(reader);
            }
            return default;
        }

        public async Task<List<T>> QueryAsync<T>(string sql, Func<IDataReader, T> map, object? parameters = null, CancellationToken ct = default)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(sql, conn);
            AddParameters(cmd, parameters);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            var list = new List<T>();
            while (await reader.ReadAsync(ct))
            {
                list.Add(map(reader));
            }
            return list;
        }

        private static void AddParameters(NpgsqlCommand cmd, object? parameters)
        {
            if (parameters == null) return;
            foreach (var prop in parameters.GetType().GetProperties())
            {
                var name = prop.Name;
                var value = prop.GetValue(parameters) ?? DBNull.Value;
                cmd.Parameters.AddWithValue(name, value);
            }
        }
    }
}

