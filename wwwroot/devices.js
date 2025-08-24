async function j(u, o = {}) {
  const r = await fetch(u, Object.assign({ headers: { 'Content-Type': 'application/json' }, credentials: 'include' }, o));
  if (!r.ok) throw 0;
  return r.json();
}

async function loadTenants(){
  try{
    const t=await fetch('/api/tenants',{credentials:'include'}).then(r=>r.json());
    const sel=document.getElementById('tenantSel'); if(!sel) return; sel.innerHTML='<option value="">All</option>' + t.map(x=>`<option value="${x.id}">${x.name}</option>`).join('');
  }catch{}
}
function getTenantQ(){ const sel=document.getElementById('tenantSel'); return (sel && sel.value) ? `?tenantId=${+sel.value}` : ''; }

async function loadDevices() {
  const list = await j('/api/devices'+getTenantQ());
  const div = document.getElementById('list');
  div.innerHTML = '';
  for (const d of list) {
    const row = document.createElement('div');
    row.className = 'card';
    row.innerHTML = `<div class="row space"><div><b>${d.clientName}</b> – ${d.circuitNumber} – ${d.ipAddress}</div><div><button class="btn" data-del="${d.id}">Delete</button></div></div>`;
    div.appendChild(row);
  }
  div.querySelectorAll('button[data-del]').forEach(b => {
    b.onclick = async () => {
      if (!confirm('Delete device?')) return;
      await fetch(`/api/devices/${b.dataset.del}`, { method:'DELETE', credentials:'include' });
      await loadDevices();
    };
  });
}

// add device
document.getElementById('add').onclick = async () => {
  const body = { ClientName: client.value.trim(), Circuit: circuit.value.trim(), Ip: ip.value.trim(), Comm: comm.value.trim(), Max: +max.value, Interval: null };
  const r = await j('/api/devices', { method:'POST', body: JSON.stringify(body) });
  if (ifx.value) { await j(`/api/devices/${r.id}/interface-index`, { method:'POST', body: JSON.stringify({ interfaceIndex: +ifx.value }) }); }
  await loadDevices();
};

// logout + initial load
document.addEventListener('DOMContentLoaded', async () => {
  await loadTenants();
  await loadDevices();
  const lb = document.getElementById('logoutBtn');
  if(lb){ lb.onclick = async()=>{ await fetch('/api/auth/logout',{method:'POST',credentials:'include'}); location.href='/login.html'; }; }
  const apply = document.getElementById('applyTenant');
  if(apply){ apply.onclick = async()=>{ await loadDevices(); }; }
});
