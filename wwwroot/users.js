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

// 2FA setup/verify/disable
async function g(u){ const r=await fetch(u,{credentials:'include'}); if(!r.ok) throw 0; return r.json(); }
async function p(u,b){ const r=await fetch(u,{method:'POST', headers:{'Content-Type':'application/json'}, credentials:'include', body:JSON.stringify(b)}); if(!r.ok) throw 0; return r.json().catch(()=>({})); }

document.getElementById('setup2fa')?.addEventListener('click', async ()=>{
  const id = +document.getElementById('uid2fa').value; if(!id) return;
  const r = await p(`/api/users/${id}/2fa/setup`,{});
  const info = document.getElementById('otpInfo');
  info.innerHTML = `Secret: <b>${r.secret||r.Secret}</b><br>Scan URI: <span class="muted">${r.otpAuthUri||r.OtpAuthUri}</span>`;
});

document.getElementById('verify2fa')?.addEventListener('click', async ()=>{
  const id = +document.getElementById('uid2fa').value; if(!id) return;
  const code = document.getElementById('otpCode').value.trim(); if(!code) return;
  await p(`/api/users/${id}/2fa/verify`,{ Code: code });
  alert('2FA enabled');
});

document.getElementById('disable2fa')?.addEventListener('click', async ()=>{
  const id = +document.getElementById('uid2fa').value; if(!id) return;
  await p(`/api/users/${id}/2fa/disable`,{});
  alert('2FA disabled');
});
