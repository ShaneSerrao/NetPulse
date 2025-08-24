async function j(u,o={}){const r=await fetch(u,Object.assign({headers:{'Content-Type':'application/json'},credentials:'include'},o));if(!r.ok)throw 0;return r.json()}

async function loadTenants(){
  try{
    const t=await fetch('/api/tenants',{credentials:'include'}).then(r=>r.json());
    const sel=document.getElementById('tenantSel'); if(!sel) return;
    sel.innerHTML='<option value="">All</option>' + t.map(x=>`<option value="${x.id}">${x.name}</option>`).join('');
  }catch{}
}
function getTenantQ(){ const sel=document.getElementById('tenantSel'); return (sel && sel.value) ? `?tenantId=${+sel.value}` : ''; }

async function loadCatalog(){
  const cat=await j('/api/mibs/catalog');
  const cs=document.getElementById('catSel'); cs.innerHTML='';
  for(const c of cat){
    const opt=document.createElement('option');
    opt.value=c.id; opt.textContent=`${c.name} (${c.oid})`;
    cs.appendChild(opt);
  }
}
async function loadDevices(){
  const devs=await j('/api/devices'+getTenantQ());
  const ds=document.getElementById('devSel'); ds.innerHTML='';
  for(const d of devs){
    const opt=document.createElement('option');
    opt.value=d.id; opt.textContent=`${d.clientName} – ${d.circuitNumber}`;
    ds.appendChild(opt);
  }
}
document.getElementById('walk').onclick=async()=>{
  const ip = document.getElementById('ip').value.trim();
  const comm = document.getElementById('comm').value.trim();
  const oid = document.getElementById('oid').value.trim();
  const r=await j('/api/mibs/walk',{method:'POST',body:JSON.stringify({ip,community:comm,baseOid:oid})});
  out.textContent = `Entries: ${r.count}\n` + (r.entries||[]).map(e=>e.line).join('\n');
};
document.getElementById('add').onclick=async()=>{
  await j('/api/mibs/catalog',{method:'POST',body:JSON.stringify({oid:coid.value.trim(),name:cname.value.trim()})});
  await loadCatalog();
};
document.getElementById('attach').onclick=async()=>{
  const devId = +devSel.value; const mibIds = [...catSel.selectedOptions].map(o=>+o.value);
  if(!devId||mibIds.length===0){ alert('Select device and OIDs'); return; }
  await j('/api/mibs/attach',{method:'POST',body:JSON.stringify({deviceId:devId,mibIds})});
  alert('Attached');
};
document.addEventListener('DOMContentLoaded', async ()=>{
  await loadTenants(); await loadDevices(); await loadCatalog();
  const lb=document.getElementById('logoutBtn'); if(lb){ lb.onclick=async()=>{ await fetch('/api/auth/logout',{method:'POST',credentials:'include'}); location.href='/login.html';}; }
  const apply=document.getElementById('applyTenant'); if(apply){ apply.onclick=async()=>{ await loadDevices(); }; }
});
