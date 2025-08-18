async function fetchJson(url, options = {}) {
  const res = await fetch(url, Object.assign({ headers: { 'Content-Type': 'application/json' }, credentials: 'include' }, options));
  if (!res.ok) throw new Error('Request failed');
  return res.json();
}

function formatMbps(n) { return (Math.round(n * 10) / 10).toFixed(1); }
function formatBytes(bytes) {
  const units = ['B','KB','MB','GB','TB'];
  let i = 0; let v = bytes;
  while (v >= 1024 && i < units.length-1) { v /= 1024; i++; }
  return `${v.toFixed(3)} ${units[i]}`;
}

async function applyTheme() {
  try {
    const t = await fetchJson('/api/settings/theme');
    const root = document.documentElement;
    root.style.setProperty('--primary', t.primary || t.Primary);
    root.style.setProperty('--accent', t.accent || t.Accent);
    root.style.setProperty('--warning', t.warning || t.Warning);
    root.style.setProperty('--danger', t.danger || t.Danger);
    root.style.setProperty('--bg', t.background || t.Background);
    root.style.setProperty('--surface', t.surface || t.Surface);
    root.style.setProperty('--text', t.text || t.Text);
  } catch {}
}

async function ensureAuth() {
  try { await fetchJson('/api/auth/me'); }
  catch { location.href = '/login.html'; }
}

async function loadDevices() {
  await ensureAuth();
  await applyTheme();
  const container = document.getElementById('devices');
  const tpl = document.getElementById('device-card');
  const devices = await fetchJson('/api/devices');
  container.innerHTML = '';
  for (const d of devices) {
    const node = tpl.content.cloneNode(true);
    const card = node.querySelector('.card');
    node.querySelector('.title').textContent = d.clientName;
    node.querySelector('.circuit').textContent = `Circuit: ${d.circuitNumber}`;
    const ipEl = node.querySelector('.ip'); ipEl.textContent = d.ipAddress; ipEl.classList.add('masked');
    const statusEl = node.querySelector('.status');
    const bar = node.querySelector('.bar-fill');
    const downEl = node.querySelector('.down');
    const upEl = node.querySelector('.up');
    const latEl = node.querySelector('.latency');
    const revealBtn = node.querySelector('.reveal');
    const snmpLink = node.querySelector('a.btn.outline'); snmpLink.href = `snmp://${d.ipAddress}`;
    const monthlyEl = node.querySelector('.monthly');

    revealBtn.addEventListener('click', () => {
      ipEl.classList.remove('masked');
      setTimeout(() => ipEl.classList.add('masked'), 15000);
    });

    async function refresh() {
      try {
        const stats = await fetchJson(`/api/devices/${d.id}/live`);
        statusEl.classList.toggle('online', stats.online);
        statusEl.classList.toggle('offline', !stats.online);
        downEl.textContent = formatMbps(stats.downloadMbps);
        upEl.textContent = formatMbps(stats.uploadMbps);
        latEl.textContent = stats.latencyMs ? Math.round(stats.latencyMs) : '-';
        bar.style.width = `${stats.linkUsagePercent}%`;
      } catch {}
    }
    async function refreshMonthly(){
      try {
        const m = await fetchJson(`/api/stats/${d.id}/monthly-usage`);
        monthlyEl.textContent = formatBytes(m.bytes);
      } catch {}
    }
    setInterval(refresh, 5000);
    setInterval(refreshMonthly, 30000);
    refresh();
    refreshMonthly();
    container.appendChild(node);
  }

  const logoutBtn = document.getElementById('logoutBtn');
  if (logoutBtn) logoutBtn.addEventListener('click', async () => {
    await fetch('/api/auth/logout', { method: 'POST', credentials: 'include' });
    location.href = '/login.html';
  });
}

window.addEventListener('DOMContentLoaded', loadDevices);

