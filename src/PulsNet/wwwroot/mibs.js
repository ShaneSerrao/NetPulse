async function j(u,o={}){const r=await fetch(u,Object.assign({headers:{'Content-Type':'application/json'},credentials:'include'},o));if(!r.ok)throw 0;return r.json()}
async function load(){ const cat=await j('/api/mibs/catalog'); document.getElementById('catalog').innerHTML = cat.map(c=>`${c.name} (${c.oid})`).join('<br>'); }
document.getElementById('walk').onclick=async()=>{ const ip=ipEl.value.trim(), comm=commEl.value.trim(), oid=oidEl.value.trim(); const r=await j('/api/mibs/walk',{method:'POST',body:JSON.stringify({ip,community:comm,baseOid:oid})}); out.textContent = `Entries: ${r.count}\n` + (r.entries||[]).map(e=>e.line).join('\n'); };
document.getElementById('add').onclick=async()=>{ await j('/api/mibs/catalog',{method:'POST',body:JSON.stringify({ oid:coid.value.trim(), name:cname.value.trim() })}); await load(); };
const ipEl=document.getElementById('ip'), commEl=document.getElementById('comm'), oidEl=document.getElementById('oid'), out=document.getElementById('out'), coid=document.getElementById('coid'), cname=document.getElementById('cname');
window.addEventListener('DOMContentLoaded', load);
