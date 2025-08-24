using System.Text.Json; using PulsNet.Data;
namespace PulsNet.Services {
  public sealed class DeviceManagementService {
    private readonly Db _db;
    public DeviceManagementService(Db db){ _db=db; }
    public async Task<long> Enqueue(string type,int userId,int[] deviceIds,object payload){
      long? nid = await _db.One(
        "INSERT INTO management_actions(action_type,initiated_by_user_id,payload_json) VALUES(@t,@u,@p) RETURNING id",
        r => (long?)r.GetInt64(0),
        new{ t=type, u=userId, p=JsonSerializer.Serialize(payload) }
      );
      var id = nid ?? 0L;
      foreach(var d in deviceIds.Distinct()) 
        await _db.Exec("INSERT INTO management_action_devices(action_id,device_id) VALUES(@a,@d)", new{ a=id, d});
      return id;
    }
    public Task RecordHistory(int deviceId,string actionType,int? userId,string? oldCfg,string? newCfg,string status,long? actionId)
      => _db.Exec("INSERT INTO \"ConfigHistory\"(\"DeviceId\",\"ActionType\",\"UserId\",\"OldConfig\",\"NewConfig\",\"Status\",\"ActionId\") VALUES(@d,@a,@u,@o,@n,@s,@x)"
        , new{ d=deviceId,a=actionType,u=userId,o=oldCfg,n=newCfg,s=status,x=actionId});
  }
}
