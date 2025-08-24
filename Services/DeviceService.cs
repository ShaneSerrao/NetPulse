using System.Net; using System.Text.RegularExpressions; using PulsNet.Data;
namespace PulsNet.Services {
  public sealed class DeviceService {
    private readonly Db _db;
    public DeviceService(Db db){ _db=db; }
    public static bool IsValidIp(string ip)=> IPAddress.TryParse(ip,out _);
    public static bool IsValidCommunity(string c)=> Regex.IsMatch(c,"^[A-Za-z0-9_.-]{1,64}$");
    public Task<List<Device>> GetAll()=> _db.Many("SELECT id,client_name,circuit_number,ip_address,snmp_community,max_link_mbps,per_client_interval_seconds FROM devices ORDER BY client_name",
      r=> new Device{ Id=r.GetInt32(0), ClientName=r.GetString(1), Circuit=r.GetString(2), Ip=r.GetString(3), Comm=r.GetString(4), Max=r.GetInt32(5), Interval=r.IsDBNull(6)?null:r.GetInt32(6)});
    public Task<Device?> Get(int id)=> _db.One("SELECT id,client_name,circuit_number,ip_address,snmp_community,max_link_mbps,per_client_interval_seconds FROM devices WHERE id=@id",
      r=> new Device{ Id=r.GetInt32(0), ClientName=r.GetString(1), Circuit=r.GetString(2), Ip=r.GetString(3), Comm=r.GetString(4), Max=r.GetInt32(5), Interval=r.IsDBNull(6)?null:r.GetInt32(6)}, new{id});
    public async Task<int> Create(Device d){
      if(!IsValidIp(d.Ip) || !IsValidCommunity(d.Comm)) throw new ArgumentException("Invalid device");
      int? nid = await _db.One(
        "INSERT INTO devices(client_name,circuit_number,ip_address,snmp_community,max_link_mbps,per_client_interval_seconds) VALUES(@a,@b,@c,@d,@e,@f) RETURNING id",
        r => (int?)r.GetInt32(0),
        new{ a=d.ClientName, b=d.Circuit, c=d.Ip, d=d.Comm, e=d.Max, f=d.Interval }
      );
      return nid ?? 0;
    }
    public Task Update(Device d){ if(!IsValidIp(d.Ip) || !IsValidCommunity(d.Comm)) throw new ArgumentException("Invalid device"); return _db.Exec("UPDATE devices SET client_name=@a,circuit_number=@b,ip_address=@c,snmp_community=@d,max_link_mbps=@e,per_client_interval_seconds=@f WHERE id=@id",
        new{ a=d.ClientName, b=d.Circuit, c=d.Ip, d=d.Comm, e=d.Max, f=d.Interval, id=d.Id}); }
    public Task Delete(int id)=> _db.Exec("DELETE FROM devices WHERE id=@id", new{id});
    public sealed class Device{ public int Id{get;set;} public string ClientName{get;set;}=""!; public string Circuit{get;set;}=""!; public string Ip{get;set;}=""!; public string Comm{get;set;}="public"; public int Max{get;set;} public int? Interval{get;set;} }
  }
}
