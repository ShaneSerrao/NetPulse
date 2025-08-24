// --- Utility fetch wrapper ---
async function j(url, opts = {}) {
  const res = await fetch(url, Object.assign({ headers: { 'Content-Type': 'application/json' }, credentials: 'include' }, opts));
  if (!res.ok) throw 0;
  return res.json();
}

// --- Format numbers ---
function fmt(n) {
  return (Math.round(n * 10) / 10).toFixed(1);
}

// --- Auth ---
async function ensureAuth() {
  try { await j('/api/auth/me'); }
  catch { location.href = '/login.html'; }
}

// --- Theme loader ---
async function loadTheme() {
  try {
    const t = await j('/api/settings/theme');
    const root = document.documentElement;
    root.style.setProperty('--primary', t.primary || t.Primary);
    root.style.setProperty('--accent', t.accent || t.Accent);
  } catch {}
}

// --- Devices Dashboard ---
async function runDashboard() {
  await ensureAuth();
  await loadTheme();

  const tenantEl = document.getElementById('tenant');
  const applyBtn = document.getElementById('applyTenant');
  let tMatch = null;

  if (applyBtn && tenantEl) {
    applyBtn.onclick = () => {
      const t = tenantEl.value ? `?tenantId=${+tenantEl.value}` : '';
      location.href = `/${t ? `#${t}` : ''}`;
      location.reload();
    };
    const hash = location.hash || '';
    tMatch = hash.startsWith('#?tenantId=') ? +hash.replace('#?tenantId=', '') : null;
  }

  const list = await j('/api/devices' + (tMatch ? `?tenantId=${tMatch}` : ''));
  const container = document.getElementById('devices');
  const template = document.getElementById('card');
  if (!container || !template) return;

  container.innerHTML = '';

  const order = JSON.parse(localStorage.getItem('cardOrder') || '[]');
  const byId = new Map(list.map(d => [d.id, d]));
  const ordered = [...order.map(id => byId.get(id)).filter(Boolean), ...list.filter(d => !order.includes(d.id))];

  for (const d of ordered) {
    const node = template.content.cloneNode(true);
    const card = node.querySelector('.card');
    card.dataset.id = String(d.id);

    node.querySelector('.title').textContent = d.clientName;
    node.querySelector('.circuit').textContent = `Circuit: ${d.circuitNumber}`;
    const ip = node.querySelector('.ip'); ip.textContent = d.ipAddress;
    const dot = node.querySelector('.dot');
    const bar = node.querySelector('.bar-fill');
    const down = node.querySelector('.down');
    const up = node.querySelector('.up');
    const lat = node.querySelector('.latency');

    // Reveal IP
    node.querySelector('.reveal')?.addEventListener('click', () => {
      ip.classList.remove('masked');
      setTimeout(() => ip.classList.add('masked'), 15000);
    });

    // Overlay metrics
    card.addEventListener('click', async (e) => {
      if (e.target.closest('.reveal')) return;
      const ov = document.getElementById('overlay');
      const body = document.getElementById('ovBody');
      const title = document.getElementById('ovTitle');
      if (!ov || !body || !title) return;

      title.textContent = `${d.clientName} – OID Metrics`;
      body.innerHTML = 'Loading...';
      ov.classList.remove('hidden');

      try {
        const vals = await j(`/api/mibs/device/${d.id}/values`);
        body.innerHTML = vals.map(v => `${v.name} (${v.oid}): ${v.value}`).join('<br>');
      } catch {
        body.textContent = 'No OIDs attached or SNMP error.';
      }
    });

    // Live stats
    async function tick() {
      try {
        const s = await j(`/api/devices/${d.id}/live`);
        dot.classList.toggle('online', s.online);
        dot.classList.toggle('offline', !s.online);
        down.textContent = fmt(s.downloadMbps);
        up.textContent = fmt(s.uploadMbps);
        lat.textContent = s.latencyMs ? Math.round(s.latencyMs) : '-';
        bar.style.width = `${s.linkUsagePercent || 0}%`;
      } catch {}
    }
    setInterval(tick, 5000);
    tick();

    container.appendChild(node);
  }

  // Overlay close
  document.getElementById('closeOverlay')?.addEventListener('click', () => {
    document.getElementById('overlay')?.classList.add('hidden');
  });

  // Drag-and-drop reordering
  let dragEl = null;
  container.addEventListener('dragstart', e => {
    const el = e.target.closest('.card');
    if (!el) return;
    dragEl = el;
    e.dataTransfer.effectAllowed = 'move';
  });
  container.addEventListener('dragover', e => {
    e.preventDefault();
    const el = e.target.closest('.card');
    if (!dragEl || !el || el === dragEl) return;
    const rect = el.getBoundingClientRect();
    const before = (e.clientY - rect.top) < rect.height / 2;
    container.insertBefore(dragEl, before ? el : el.nextSibling);
  });
  container.addEventListener('dragend', () => { dragEl = null; saveOrder(); });
  [...container.querySelectorAll('.card')].forEach(el => el.setAttribute('draggable', 'true'));

  function saveOrder() {
    const ids = [...container.querySelectorAll('.card')].map(el => +el.dataset.id);
    localStorage.setItem('cardOrder', JSON.stringify(ids));
  }

  // Logout
  const lb = document.getElementById('logoutBtn');
  if(lb) lb.onclick = async () => { await fetch('/api/auth/logout', {method:'POST', credentials:'include'}); location.href='/login.html'; };
}

// --- Devices Page Add/List/Delete ---
async function loadDevices() {
  const div = document.getElementById('list');
  if(!div) return;

  const list = await j('/api/devices');
  div.innerHTML = '';

  for (const d of list) {
    const row = document.createElement('div');
    row.className = 'card';
    row.innerHTML = `<div class="row space">
      <div><b>${d.clientName}</b> – ${d.circuitNumber} – ${d.ipAddress}</div>
      <div><button class="btn" data-del="${d.id}">Delete</button></div>
    </div>`;
    div.appendChild(row);
  }

  div.querySelectorAll('button[data-del]').forEach(b => {
    b.onclick = async () => {
      if(!confirm('Delete device?')) return;
      await fetch(`/api/devices/${b.dataset.del}`, { method:'DELETE', credentials:'include' });
      await loadDevices();
    };
  });
}

document.getElementById('add')?.addEventListener('click', async () => {
  const body = {
    ClientName: client.value.trim(),
    Circuit: circuit.value.trim(),
    Ip: ip.value.trim(),
    Comm: comm.value.trim(),
    Max: +max.value,
    Interval: null
  };
  const r = await j('/api/devices', { method:'POST', body: JSON.stringify(body) });
  if(ifx.value) await j(`/api/devices/${r.id}/interface-index`, { method:'POST', body: JSON.stringify({ interfaceIndex: +ifx.value }) });
  await loadDevices();
});

// --- Management Page Device Picker & Actions ---
function selected() { return [...document.querySelectorAll('input[name=dev]:checked')].map(x => +x.value); }

async function loadDevs() {
  const devs = await j('/api/devices');
  const d = document.getElementById('devList');
  if(!d) return;
  d.innerHTML = '';
  for (const v of devs) {
    const row = document.createElement('div');
    row.className = 'row';
    row.innerHTML = `<label><input type="checkbox" name="dev" value="${v.id}"> ${v.clientName} – ${v.circuitNumber} – ${v.ipAddress}</label>`;
    d.appendChild(row);
  }
}

document.getElementById('applyTpl')?.addEventListener('click', async () => {
  const r = await j('/api/management/apply-template', { method:'POST', body: JSON.stringify({ templateId: +tpl.value, deviceIds: selected() }) });
  out.textContent = `Action: ${r.actionId}`;
});

document.getElementById('runScr')?.addEventListener('click', async () => {
  const r = await j('/api/management/run-script', { method:'POST', body: JSON.stringify({ scriptId: +scr.value, deviceIds: selected() }) });
  out.textContent = `Action: ${r.actionId}`;
});

document.getElementById('fwBtn')?.addEventListener('click', async () => {
  const r = await j('/api/management/firmware', { method:'POST', body: JSON.stringify({ firmwareVersion: fw.value.trim(), deviceIds: selected() }) });
  out.textContent = `Action: ${r.actionId}`;
});

// --- Dynamic active nav link ---
document.addEventListener('DOMContentLoaded', () => {
  const path = window.location.pathname;
  document.querySelectorAll('.topbar nav a').forEach(a => {
    a.classList.remove('active');
    if(a.getAttribute('href') === path) a.classList.add('active');
  });

  // Attach universal logout handler
  const lb = document.getElementById('logoutBtn');
  if(lb) lb.onclick = async () => { await fetch('/api/auth/logout', {method:'POST', credentials:'include'}); location.href='/login.html'; };

  // Load devices if on devices page
  if(document.getElementById('list')) loadDevices();

  // Load management devices if on management page
  if(document.getElementById('devList')) loadDevs();

  // Run dashboard if on dashboard page
  if(document.getElementById('devices')) runDashboard();
});
