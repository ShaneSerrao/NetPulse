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

  // Load layout versions
  try{
    const vers = await g('/api/layout/versions');
    const sel = document.getElementById('layoutVersions');
    sel.innerHTML = '';
    for(const v of vers){ const o=document.createElement('option'); o.value=v.name; o.textContent=v.name; sel.appendChild(o);}    
  }catch{}
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
  // Layout advanced controls
  const saveLayout= document.getElementById('saveLayout'); if(saveLayout){ saveLayout.onclick= async()=>{ const code= document.getElementById('layoutCode').value; const name= (document.getElementById('layoutVersions').value)||'Default'; await p('/api/layout/versions',{ Name:name, Code:code}); await load(); alert('Layout saved'); } }
  const saveLayoutAs= document.getElementById('saveLayoutAs'); if(saveLayoutAs){ saveLayoutAs.onclick= async()=>{ const code= document.getElementById('layoutCode').value; const name= prompt('Save layout as name:'); if(!name) return; await p('/api/layout/versions',{ Name:name, Code:code}); await load(); } }
  const useSel= document.getElementById('useSelectedLayout'); if(useSel){ useSel.onclick= async()=>{ const sel=document.getElementById('layoutVersions'); const name= sel.value; if(!name){ alert('Select a version'); return;} alert('Selected layout will be used on next reload.'); } }
  const delSel= document.getElementById('deleteSelectedLayout'); if(delSel){ delSel.onclick= async()=>{ const sel=document.getElementById('layoutVersions'); const name= sel.value; if(!name){ alert('Select a version'); return;} if(name==='Default'){ alert('Cannot delete Default'); return;} await p('/api/layout/versions/delete',{ Name:name}); await load(); } }
});
