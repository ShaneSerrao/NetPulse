async function j(url, opts = {}) {
  const res = await fetch(url, Object.assign({ headers: { 'Content-Type': 'application/json' }, credentials: 'include' }, opts));
  if (!res.ok) throw 0;
  return res.json();
}

// Load all templates
async function loadTemplates() {
  const div = document.getElementById('tplList');
  div.innerHTML = '';
  const list = await j('/api/templates');
  for (const t of list) {
    const row = document.createElement('div');
    row.className = 'row space';
    row.innerHTML = `<div><b>${t.name}</b> – ${t.description}</div>
                     <div><button class="btn" data-del="${t.id}">Delete</button></div>`;
    div.appendChild(row);
  }

  // Delete handler
  div.querySelectorAll('button[data-del]').forEach(b => {
    b.onclick = async () => {
      if (!confirm('Delete template?')) return;
      await j(`/api/templates/${b.dataset.del}`, { method:'DELETE' });
      await loadTemplates();
    };
  });
}

// Add template
document.getElementById('addTpl')?.addEventListener('click', async () => {
  const body = {
    name: tplName.value.trim(),
    description: tplDesc.value.trim()
  };
  await j('/api/templates', { method:'POST', body: JSON.stringify(body) });
  tplName.value = tplDesc.value = '';
  await loadTemplates();
});

// Logout + initial load
document.addEventListener('DOMContentLoaded', async () => {
  await loadTemplates();
  const lb = document.getElementById('logoutBtn');
  if(lb) lb.onclick = async () => { await fetch('/api/auth/logout', {method:'POST', credentials:'include'}); location.href='/login.html'; };
});
