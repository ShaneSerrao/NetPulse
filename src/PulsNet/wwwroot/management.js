async function j(u, o = {}) {
  const r = await fetch(u, Object.assign({ headers: { 'Content-Type': 'application/json' }, credentials: 'include' }, o));
  if (!r.ok) throw 0;
  return r.json();
}

function ids() {
  return devs.value.split(',').map(s => +s.trim()).filter(Boolean);
}

document.getElementById('applyTpl').onclick = async () => {
  const r = await j('/api/management/apply-template', {
    method: 'POST',
    body: JSON.stringify({ templateId: +tpl.value, deviceIds: ids() })
  });
  out.textContent = `Action: ${r.actionId}`;
};

document.getElementById('runScr').onclick = async () => {
  const r = await j('/api/management/run-script', {
    method: 'POST',
    body: JSON.stringify({ scriptId: +scr.value, deviceIds: ids() })
  });
  out.textContent = `Action: ${r.actionId}`;
};

document.getElementById('fwBtn').onclick = async () => {
  const r = await j('/api/management/firmware', {
    method: 'POST',
    body: JSON.stringify({ firmwareVersion: fw.value.trim(), deviceIds: ids() })
  });
  out.textContent = `Action: ${r.actionId}`;
};
