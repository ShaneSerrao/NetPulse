async function j(u, o = {}) {
  const r = await fetch(u, Object.assign({ headers: { 'Content-Type': 'application/json' }, credentials: 'include' }, o));
  if (!r.ok) throw 0;
  return r.json();
}

async function load() {
  const list = await j('/api/tenants');
  const div = document.getElementById('list');
  div.innerHTML = '';
  for (const t of list) {
    const row = document.createElement('div');
    row.className = 'row space';
    row.innerHTML = `<div>${t.name}</div>`;
    div.appendChild(row);
  }
}

document.getElementById('create').onclick = async () => {
  await j('/api/tenants', { method: 'POST', body: JSON.stringify({ name: name.value.trim() }) });
  await load();
};

window.addEventListener('DOMContentLoaded', load);
