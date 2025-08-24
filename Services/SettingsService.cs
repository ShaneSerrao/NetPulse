using System.Text.Json; using PulsNet.Data;
namespace PulsNet.Services {
  public sealed class SettingsService {
    private readonly Db _db; private readonly ConfigService _cfg;
    public SettingsService(Db db, ConfigService cfg){ _db=db; _cfg=cfg; }
    public async Task<ThemeConfig> GetTheme(){ var j= await _db.One("SELECT value_json FROM settings WHERE key='theme'", r=> r.GetString(0)); return string.IsNullOrWhiteSpace(j)? _cfg.Config.Theme : (JsonSerializer.Deserialize<ThemeConfig>(j!) ?? _cfg.Config.Theme); }
    public Task SaveTheme(ThemeConfig t)=> _db.Exec("INSERT INTO settings(key,value_json) VALUES('theme',@j) ON CONFLICT(key) DO UPDATE SET value_json=EXCLUDED.value_json", new{ j=JsonSerializer.Serialize(t)});
    public async Task<(int interval,int cache,int offline)> GetPolling(){ var j= await _db.One("SELECT value_json FROM settings WHERE key='polling'", r=> r.GetString(0)); if(string.IsNullOrWhiteSpace(j)){ var p=_cfg.Config.Polling; return (p.GlobalIntervalSeconds,p.CacheSeconds,p.OfflineThresholdSeconds);} var d=JsonSerializer.Deserialize<PollWrap>(j!)!; return (d.GlobalIntervalSeconds,d.CacheSeconds,d.OfflineThresholdSeconds);}
    public Task SavePolling(int interval,int cache,int offline)=> _db.Exec("INSERT INTO settings(key,value_json) VALUES('polling',@j) ON CONFLICT(key) DO UPDATE SET value_json=EXCLUDED.value_json", new{ j=JsonSerializer.Serialize(new PollWrap{GlobalIntervalSeconds=interval,CacheSeconds=cache,OfflineThresholdSeconds=offline})});
    private sealed class PollWrap{ public int GlobalIntervalSeconds{get;set;} public int CacheSeconds{get;set;} public int OfflineThresholdSeconds{get;set;} }
  }
}
