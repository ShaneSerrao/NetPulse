async function postJson(url, data) {
  const res = await fetch(url, { method: 'POST', headers: { 'Content-Type': 'application/json' }, credentials: 'include', body: JSON.stringify(data) });
  if (!res.ok) throw new Error((await res.json()).error || 'Login failed');
  return res.json();
}

document.getElementById('loginForm').addEventListener('submit', async (e) => {
  e.preventDefault();
  const username = document.getElementById('username').value.trim();
  const password = document.getElementById('password').value;
  const totp = document.getElementById('totp').value.trim();
  const errorEl = document.getElementById('error');
  errorEl.classList.add('hidden');
  try {
    const res = await postJson('/api/auth/login', { username, password, totpCode: totp || undefined });
    location.href = '/';
  } catch (err) {
    errorEl.textContent = err.message;
    errorEl.classList.remove('hidden');
  }
});

