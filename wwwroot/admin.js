async function g(u){
  const r = await fetch(u,{credentials:'include'});
  if(!r.ok) throw 0;
  return r.json();
}

async function p(u,b){
  const r = await fetch(u,{
    method:'POST',
    headers:{'Content-Type':'application/json'},
    credentials:'include',
    body:JSON.stringify(b)
  });
  if(!r.ok) throw 0;
  return r.json().catch(()=>({}));
}

async function load(){
  const t = await g('/api/settings/theme');
  mode.value=(t.name||t.Name)||'dark';
  primary.value=t.primary||t.Primary||'#2a3867';
  accent.value=t.accent||t.Accent||'#aeb9ff';
  const poll=await g('/api/settings/polling');
  interval.value=poll.globalIntervalSeconds;
  cache.value=poll.cacheSeconds;
  offline.value=poll.offlineThresholdSeconds;
}

async function saveTheme(){
  await p('/api/settings/theme',{Name:mode.value,Primary:primary.value,Accent:accent.value});
  alert('Saved theme');
}

async function savePoll(){
  await p('/api/settings/polling',{
    GlobalIntervalSeconds:+interval.value,
    CacheSeconds:+cache.value,
    OfflineThresholdSeconds:+offline.value
  });
  alert('Saved polling');
}

// --- 2FA load/save ---
async function load2fa(){
  const s = await g('/api/settings/global2fa');
  document.getElementById('g2fa').checked = !!s.enabled;
}

async function save2fa(){
  await p('/api/settings/global2fa',{enabled:document.getElementById('g2fa').checked});
  alert('Saved 2FA');
}

document.addEventListener('DOMContentLoaded',()=>{
  load();
  load2fa();
  document.getElementById('saveTheme').onclick = saveTheme;
  document.getElementById('savePolling').onclick = savePoll;
  document.getElementById('save2fa').onclick = save2fa;
});

// Device caps (existing)
document.getElementById('applyCaps').onclick = async ()=>{
  const id= +document.getElementById('did').value;
  const ifx = +document.getElementById('ifx').value;
  const cden = document.getElementById('cden').checked;
  const cd = document.getElementById('cd').value? +document.getElementById('cd').value : null;
  const cuen = document.getElementById('cuen').checked;
  const cu = document.getElementById('cu').value? +document.getElementById('cu').value : null;
  if(id && ifx){
    await fetch(`/api/devices/${id}/interface-index`,{
      method:'POST',
      headers:{'Content-Type':'application/json'},
      credentials:'include',
      body: JSON.stringify({ interfaceIndex: ifx })
    });
  }
  if(id){
    await fetch(`/api/devices/${id}/caps`,{
      method:'POST',
      headers:{'Content-Type':'application/json'},
      credentials:'include',
      body: JSON.stringify({ capDownEnabled:cden, capDownMbps:cd, capUpEnabled:cuen, capUpMbps:cu })
    });
    alert('Applied');
  }
};
