using Microsoft.Extensions.Hosting; using Microsoft.Extensions.Logging; using PulsNet.Data;
namespace PulsNet.Services {
  public sealed class ActionProcessorService : BackgroundService {
    private readonly ILogger<ActionProcessorService> _log; private readonly Db _db; private readonly DeviceManagementService _mgmt;
    public ActionProcessorService(ILogger<ActionProcessorService> log, Db db, DeviceManagementService mgmt){ _log=log; _db=db; _mgmt=mgmt; }
    protected override async Task ExecuteAsync(CancellationToken ct){
      while(!ct.IsCancellationRequested){
        try{
          var a = await _db.One("SELECT id, action_type, initiated_by_user_id, payload_json FROM management_actions WHERE status='Queued' ORDER BY created_utc LIMIT 1",
            r=> new { id=r.GetInt64(0), type=r.GetString(1), userId=r.IsDBNull(2)?(int?)null:r.GetInt32(2), payload=r.GetString(3)});
          if(a==null){ await Task.Delay(1000,ct); continue; }
          await _db.Exec("UPDATE management_actions SET status='Running', started_utc=now() WHERE id=@id", new{ a.id});
          var devs = await _db.Many("SELECT device_id FROM management_action_devices WHERE action_id=@id", r=> r.GetInt32(0), new{ a.id});
          int done=0;
          foreach(var d in devs){
            await Task.Delay(500,ct); done++; var p=(int)Math.Round(done*100.0/Math.Max(1,devs.Count));
            await _db.Exec("UPDATE management_actions SET progress_percent=@p WHERE id=@id", new{ p, a.id});
            await _mgmt.RecordHistory(d,a.type,a.userId,null,a.payload,"Applied",a.id);
          }
          await _db.Exec("UPDATE management_actions SET status='Completed', completed_utc=now(), progress_percent=100 WHERE id=@id", new{ a.id});
        }catch(Exception ex){ _log.LogError(ex,"ActionProcessor error"); await Task.Delay(1000,ct); }
      }
    }
  }
}
