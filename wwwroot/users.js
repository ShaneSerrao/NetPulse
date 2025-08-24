async function j(u, o = {}) {
  const r = await fetch(u, Object.assign({ headers: { 'Content-Type': 'application/json' }, credentials: 'include' }, o));
  if (!r.ok) throw 0;
  return r.json();
}

async function load() {
  const list = await j('/api/users');
  const div = document.getElementById('list');
  div.innerHTML = '';
  for (const u of list) {
    const row = document.createElement('div');
    row.className = 'row space';
    row.innerHTML = `<div>${u.username} (${u.role})</div>`;
    div.appendChild(row);
  }
}

document.getElementById('create').onclick = async () => {
  await j('/api/users', {
    method: 'POST',
    body: JSON.stringify({
      username: u.value.trim(),
      password: p.value,
      role: r.value,
      email: e.value.trim() || null
    })
  });
  await load();
};

window.addEventListener('DOMContentLoaded', load);
