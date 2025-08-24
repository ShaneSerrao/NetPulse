async function j(url, opts = {}) {
  const res = await fetch(url, Object.assign({ headers: { 'Content-Type': 'application/json' }, credentials: 'include' }, opts));
  if (!res.ok) throw 0;
  return res.json();
}

// Load scripts
async function loadScripts() {
  const div = document.getElementById('scrList');
  div.innerHTML = '';
  const list = await j('/api/scripts');
  for (const s of list) {
    const row = document.createElement('div');
    row.className = 'row space';
    row.innerHTML = `<div><b>${s.name}</b> – ${s.command}</div>
                     <div><button class="btn" data-del="${s.id}">Delete</button></div>`;
    div.appendChild(row);
  }

  div.querySelectorAll('button[data-del]').forEach(b => {
    b.onclick = async () => {
      if(!confirm('Delete script?')) return;
      await j(`/api/scripts/${b.dataset.del}`, { method:'DELETE' });
      await loadScripts();
    };
  });
}

// Add script
document.getElementById('addScr')?.addEventListener('click', async () => {
  const body = { name: scrName.value.trim(), command: scrCmd.value.trim() };
  await j('/api/scripts', { method:'POST', body: JSON.stringify(body) });
  scrName.value = scrCmd.value = '';
  await loadScripts();
});

// Logout + initial load
document.addEventListener('DOMContentLoaded', async () => {
  await loadScripts();
  const lb = document.getElementById('logoutBtn');
  if(lb) lb.onclick = async () => { await fetch('/api/auth/logout', {method:'POST', credentials:'include'}); location.href='/login.html'; };
});
