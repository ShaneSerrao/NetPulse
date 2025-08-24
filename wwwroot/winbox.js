async function j(u,o={}){ const r=await fetch(u,Object.assign({headers:{'Content-Type':'application/json'},credentials:'include'},o)); if(!r.ok) throw 0; return r.json(); }
function out(s){ const el=document.getElementById('wbout'); el.textContent = s; }

document.addEventListener('DOMContentLoaded', ()=>{
  const host=()=> document.getElementById('rhost').value.trim();
  const user=()=> document.getElementById('ruser').value.trim();
  document.getElementById('loadInterfaces').onclick = async ()=>{ const r= await j('/api/routeros/interfaces',{ method:'POST', body: JSON.stringify({ Host:host(), User:user() })}); out(r.lines.join('\n')); };
  document.getElementById('loadRoutes').onclick = async ()=>{ const r= await j('/api/routeros/routes',{ method:'POST', body: JSON.stringify({ Host:host(), User:user() })}); out(r.lines.join('\n')); };
  document.getElementById('loadQueues').onclick = async ()=>{ const r= await j('/api/routeros/queues',{ method:'POST', body: JSON.stringify({ Host:host(), User:user() })}); out(r.lines.join('\n')); };
  document.getElementById('loadLogs').onclick = async ()=>{ const r= await j('/api/routeros/logs',{ method:'POST', body: JSON.stringify({ Host:host(), User:user() })}); out(r.lines.join('\n')); };
});

