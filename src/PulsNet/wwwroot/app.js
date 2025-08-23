async function j(u, o = {}) {
  const r = await fetch(u, Object.assign({ credentials: 'include', headers: { 'Content-Type': 'application/json' } }, o));
  if (!r.ok) throw 0;
  return r.json();
}

function fmt(n) { return (Math.round(n * 10) / 10).toFixed(1); }

async function ensure() {
  try { await j('/api/auth/me'); }
  catch { location.href = '/login.html'; }
}

async function loadTheme() {
  try {
    const t = await j('/api/settings/theme');
    const root = document.documentElement;
    root.style.setProperty('--primary', t.primary || t.Primary);
    root.style.setProperty('--accent', t.accent || t.Accent);
  } catch {}
}

async function run() {
  await ensure();
  await loadTheme();

  // --- Tenant filter integration ---
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

  // Fetch devices (with tenant filter if set)
  const list = await j('/api/devices' + (tMatch ? `?tenantId=${tMatch}` : ''));
  const c = document.getElementById('devices');
  const tpl = document.getElementById('card');
  c.innerHTML = '';

  // restore order
  const order = JSON.parse(localStorage.getItem('cardOrder') || '[]');
  const byId = new Map(list.map(d => [d.id, d]));
  const ordered = [...order.map(id => byId.get(id)).filter(Boolean), ...list.filter(d => !order.includes(d.id))];

  for (const d of ordered) {
    const node = tpl.content.cloneNode(true);
    const card = node.querySelector('.card');
    card.dataset.id = String(d.id);
    node.querySelector('.title').textContent = d.clientName;
    node.querySelector('.circuit').textContent = `Circuit: ${d.circuitNumber}`;
    const ip = node.querySelector('.ip'); ip.textContent = d.ipAddress;
    const dot = node.querySelector('.dot'); const bar = node.querySelector('.bar-fill');
    const down = node.querySelector('.down'); const up = node.querySelector('.up'); const lat = node.querySelector('.latency');

    // reveal IP
    node.querySelector('.reveal').addEventListener('click', () => {
      ip.classList.remove('masked');
      setTimeout(() => ip.classList.add('masked'), 15000);
    });

    // open overlay on card click
    card.addEventListener('click', async (e) => {
      if (e.target.closest('.reveal')) return;
      const ov = document.getElementById('overlay');
      const body = document.getElementById('ovBody');
      const title = document.getElementById('ovTitle');
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

    // live stats tick
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
    c.appendChild(node);
  }

  // close overlay
  document.getElementById('closeOverlay').onclick = () => document.getElementById('overlay').classList.add('hidden');

  // drag-and-drop (simple)
  let dragEl = null;
  c.addEventListener('dragstart', e => {
    const el = e.target.closest('.card');
    if (!el) return;
    dragEl = el;
    e.dataTransfer.effectAllowed = 'move';
  });
  c.addEventListener('dragover', e => {
    e.preventDefault();
    const el = e.target.closest('.card');
    if (!dragEl || !el || el === dragEl) return;
    const rect = el.getBoundingClientRect();
    const before = (e.clientY - rect.top) < rect.height / 2;
    c.insertBefore(dragEl, before ? el : el.nextSibling);
  });
  c.addEventListener('dragend', () => { dragEl = null; saveOrder(); });
  [...c.querySelectorAll('.card')].forEach(el => el.setAttribute('draggable', 'true'));

  function saveOrder() {
    const ids = [...c.querySelectorAll('.card')].map(el => +el.dataset.id);
    localStorage.setItem('cardOrder', JSON.stringify(ids));
  }
}

window.addEventListener('DOMContentLoaded', run);
