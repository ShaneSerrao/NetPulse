async function g(u){const r=await fetch(u,{credentials:'include'});if(!r.ok)throw 0;return r.json()}
async function p(u,b){const r=await fetch(u,{method:'POST',headers:{'Content-Type':'application/json'},credentials:'include',body:JSON.stringify(b)});if(!r.ok)throw 0;return r.json().catch(()=>({}))}

async function load(){
  const t=await g('/api/settings/theme');
  mode.value=(t.name||t.Name)||'dark';
  primary.value=t.primary||t.Primary||'#2a3867';
  accent.value=t.accent||t.Accent||'#aeb9ff';
  const poll=await g('/api/settings/polling');
  interval.value=poll.globalIntervalSeconds; cache.value=poll.cacheSeconds; offline.value=poll.offlineThresholdSeconds;
  const s=await g('/api/settings/global2fa'); g2fa.checked=!!s.enabled;
}
async function saveTheme(){ await p('/api/settings/theme',{Name:mode.value,Primary:primary.value,Accent:accent.value}); alert('Theme saved'); }
async function savePoll(){ await p('/api/settings/polling',{GlobalIntervalSeconds:+interval.value,CacheSeconds:+cache.value,OfflineThresholdSeconds:+offline.value}); alert('Polling saved'); }
async function save2fa(){ await p('/api/settings/global2fa',{enabled:g2fa.checked}); alert('Security saved'); }
async function resetLayout(){ await p('/api/layout/cards',{ids:[]}); alert('Layout reset'); }

document.addEventListener('DOMContentLoaded', ()=>{
  load();
  document.getElementById('saveTheme').onclick=saveTheme;
  document.getElementById('savePolling').onclick=savePoll;
  document.getElementById('save2fa').onclick=save2fa;
  document.getElementById('resetLayout').onclick=resetLayout;
  const lb=document.getElementById('logoutBtn'); if(lb){ lb.onclick=async()=>{ await fetch('/api/auth/logout',{method:'POST',credentials:'include'}); location.href='/login.html';}; }
});
