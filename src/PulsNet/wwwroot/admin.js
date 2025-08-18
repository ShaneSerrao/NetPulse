async function fetchJson(url){ const r=await fetch(url,{credentials:'include'}); if(!r.ok) throw new Error('failed'); return r.json(); }
async function postJson(url, data){ const r=await fetch(url,{method:'POST', headers:{'Content-Type':'application/json'}, credentials:'include', body:JSON.stringify(data)}); if(!r.ok) throw new Error('failed'); return r.json().catch(()=>({})); }

async function loadTheme(){ const t = await fetchJson('/api/settings/theme'); for(const k of ['primary','accent','warning','danger','background','surface','text']){ const el=document.getElementById(k); if(el) el.value = t[k.charAt(0).toUpperCase()+k.slice(1)] || t[k]; } }
async function saveTheme(){ const theme = { Primary: primary.value, Accent: accent.value, Warning: warning.value, Danger: danger.value, Background: background.value, Surface: surface.value, Text: text.value }; await postJson('/api/settings/theme', theme); alert('Theme saved'); }

async function loadPolling(){ const p = await fetchJson('/api/settings/polling'); interval.value=p.globalIntervalSeconds; cache.value=p.cacheSeconds; offline.value=p.offlineThresholdSeconds; }
async function savePolling(){ await postJson('/api/settings/polling', { GlobalIntervalSeconds:+interval.value, CacheSeconds:+cache.value, OfflineThresholdSeconds:+offline.value }); alert('Polling saved'); }

async function loadSecurity(){ const s = await fetchJson('/api/settings/global2fa'); g2fa.checked = !!s.enabled; }
async function saveSecurity(){ await postJson('/api/settings/global2fa', { enabled: g2fa.checked }); alert('Security saved'); }

document.getElementById('saveTheme').addEventListener('click', saveTheme);
document.getElementById('savePolling').addEventListener('click', savePolling);
document.getElementById('save2fa').addEventListener('click', saveSecurity);

document.getElementById('userForm').addEventListener('submit', async (e)=>{
  e.preventDefault();
  await postJson('/api/admin/user', { username:u.value.trim(), password:p.value, role:r.value, email:e.value.trim()||undefined });
  alert('User created');
});

document.getElementById('deviceForm').addEventListener('submit', async (e)=>{
  e.preventDefault();
  const body={ ClientName:client.value.trim(), CircuitNumber:circuit.value.trim(), IpAddress:ip.value.trim(), SnmpCommunity:comm.value.trim(), MaxLinkMbps:+max.value, PerClientIntervalSeconds:pci.value?+pci.value:null };
  await postJson('/api/devices', body);
  alert('Device added');
});

async function ensureAdmin(){
  const me = await fetchJson('/api/auth/me');
  if (me.role !== 'Admin') { location.href = '/'; }
}

window.addEventListener('DOMContentLoaded', async ()=>{ await ensureAdmin(); await loadTheme(); await loadPolling(); await loadSecurity(); });

