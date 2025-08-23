using Npgsql; using System.Data; using PulsNet.Services;
namespace PulsNet.Data {
  public sealed class Db {
    private readonly string _cs;
    public Db(ConfigService cfg){
      var c=cfg.Config.Database;
      _cs=new NpgsqlConnectionStringBuilder{Host=c.Host,Port=c.Port,Database=c.Database,Username=c.Username,Password=c.Password}.ToString();
    }
    public async Task<int> Exec(string sql, object? p=null){ await using var cn=new NpgsqlConnection(_cs); await cn.OpenAsync(); await using var cmd=new NpgsqlCommand(sql,cn); Add(cmd,p); return await cmd.ExecuteNonQueryAsync(); }
    public async Task<T?> One<T>(string sql, Func<IDataReader,T> map, object? p=null){ await using var cn=new NpgsqlConnection(_cs); await cn.OpenAsync(); await using var cmd=new NpgsqlCommand(sql,cn); Add(cmd,p); await using var r=await cmd.ExecuteReaderAsync(); return await r.ReadAsync()? map(r): default; }
    public async Task<List<T>> Many<T>(string sql, Func<IDataReader,T> map, object? p=null){ await using var cn=new NpgsqlConnection(_cs); await cn.OpenAsync(); await using var cmd=new NpgsqlCommand(sql,cn); Add(cmd,p); await using var r=await cmd.ExecuteReaderAsync(); var list=new List<T>(); while(await r.ReadAsync()) list.Add(map(r)); return list; }
    private static void Add(NpgsqlCommand cmd, object? p){ if(p==null) return; foreach(var prop in p.GetType().GetProperties()){ cmd.Parameters.AddWithValue(prop.Name, prop.GetValue(p)??DBNull.Value);} }
  }
}
