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

async function load() {
    const list = await j('/api/sites'+getTenantQ());
    const div = document.getElementById('list');
    div.innerHTML = '';
    for (const s of list) {
        const row = document.createElement('div');
        row.className = 'row space';
        row.innerHTML = `<div>${s.name} (${s.latitude??'-'}, ${s.longitude??'-'})</div>`;
        div.appendChild(row);
        drawPin(s.latitude,s.longitude);
    }
}

const nameEl=name, addrEl=addr, latEl=lat, lonEl=lon;
document.getElementById('save').onclick = async () => {
    await j('/api/sites', { method:'POST', body: JSON.stringify({ name: nameEl.value.trim(), address: addrEl.value.trim()||null, latitude: latEl.value?+latEl.value:null, longitude: lonEl.value?+lonEl.value:null }) });
    nameEl.value=addrEl.value=latEl.value=lonEl.value='';
    await load();
};

const map=document.getElementById('map');
const ctx=map.getContext('2d');

function drawBase(){ ctx.fillStyle='#0a0a0a'; ctx.fillRect(0,0,map.width,map.height); ctx.strokeStyle='#222'; for(let x=0;x<map.width;x+=50){ ctx.beginPath(); ctx.moveTo(x,0); ctx.lineTo(x,map.height); ctx.stroke(); } for(let y=0;y<map.height;y+=50){ ctx.beginPath(); ctx.moveTo(0,y); ctx.lineTo(map.width,y); ctx.stroke(); } }
function drawPin(lat,lon){ if(lat==null||lon==null) return; const px = (lon+180)/360*map.width; const py = (90-lat)/180*map.height; ctx.fillStyle='#aeb9ff'; ctx.beginPath(); ctx.arc(px,py,5,0,Math.PI*2); ctx.fill(); }

map.addEventListener('click',(e)=>{ const r=map.getBoundingClientRect(); const px=e.clientX-r.left; const py=e.clientY-r.top; const lon = (px/map.width)*360 - 180; const lat = 90 - (py/map.height)*180; latEl.value=lat.toFixed(6); lonEl.value=lon.toFixed(6); });

window.addEventListener('DOMContentLoaded', async ()=>{
    drawBase();
    await load();
    await loadTenants();
    const apply=document.getElementById('applyTenant'); if(apply){ apply.onclick=async()=>{ await load(); }; }
    const lb=document.getElementById('logoutBtn'); if(lb){ lb.onclick=async()=>{ await fetch('/api/auth/logout',{method:'POST',credentials:'include'}); location.href='/login.html';}; }
});
