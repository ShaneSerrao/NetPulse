async function j(u, o = {}) {
  const r = await fetch(u, Object.assign({ headers: { 'Content-Type': 'application/json' }, credentials: 'include' }, o));
  if (!r.ok) throw 0;
  return r.json();
}

// get selected device IDs
function selected() {
  return [...document.querySelectorAll('input[name=dev]:checked')].map(x => +x.value);
}

// load devices into checkboxes
async function loadDevs() {
  const devs = await j('/api/devices');
  const d = document.getElementById('devList');
  d.innerHTML = '';
  for (const v of devs) {
    const row = document.createElement('div');
    row.className = 'row';
    row.innerHTML = `<label><input type="checkbox" name="dev" value="${v.id}"> ${v.clientName} – ${v.circuitNumber} – ${v.ipAddress}</label>`;
    d.appendChild(row);
  }
}

// bulk actions
document.getElementById('applyTpl').onclick = async () => {
  const r = await j('/api/management/apply-template', {
    method: 'POST',
    body: JSON.stringify({ templateId: +tpl.value, deviceIds: selected() })
  });
  out.textContent = `Action: ${r.actionId}`;
};

document.getElementById('runScr').onclick = async () => {
  const r = await j('/api/management/run-script', {
    method: 'POST',
    body: JSON.stringify({ scriptId: +scr.value, deviceIds: selected() })
  });
  out.textContent = `Action: ${r.actionId}`;
};

document.getElementById('fwBtn').onclick = async () => {
  const r = await j('/api/management/firmware', {
    method: 'POST',
    body: JSON.stringify({ firmwareVersion: fw.value.trim(), deviceIds: selected() })
  });
  out.textContent = `Action: ${r.actionId}`;
};

// logout + load devices
document.addEventListener('DOMContentLoaded', () => {
  loadDevs();
  const lb = document.getElementById('logoutBtn');
  if(lb) lb.onclick = async () => { await fetch('/api/auth/logout', {method:'POST', credentials:'include'}); location.href='/login.html'; };
});
