async function fetchJson(url){ const r=await fetch(url,{credentials:'include'}); if(!r.ok) throw new Error('failed'); return r.json(); }
function card(item){
  const div = document.createElement('div');
  div.className = 'card';
  div.innerHTML = `<div class="row space"><div class="title">${item.clientName}</div><div class="dot offline"></div></div>
  <div class="muted">Circuit: ${item.circuitNumber}</div>
  <div class="ip masked">${item.ip}</div>`;
  return div;
}
async function applyTheme(){
  try { const t = await fetch('/api/settings/theme',{credentials:'include'}).then(r=>r.json()); const root=document.documentElement; root.style.setProperty('--primary', t.primary||t.Primary); root.style.setProperty('--accent', t.accent||t.Accent); root.style.setProperty('--warning', t.warning||t.Warning); root.style.setProperty('--danger', t.danger||t.Danger); root.style.setProperty('--bg', t.background||t.Background); root.style.setProperty('--surface', t.surface||t.Surface); root.style.setProperty('--text', t.text||t.Text);} catch {}
}

async function ensureAuth(){ try{ await fetch('/api/auth/me',{credentials:'include'}); } catch { location.href='/login.html'; } }

async function run(){
  await ensureAuth();
  await applyTheme();
  const list = await fetchJson('/api/offline');
  const container = document.getElementById('list');
  container.innerHTML = '';
  for(const item of list) container.appendChild(card(item));
}
window.addEventListener('DOMContentLoaded', run);

