function wsUrl(path){ const h=location.host; const proto = location.protocol==='https:'?'wss':'ws'; return `${proto}://${h}${path}`; }
let sock=null; let termEl=null; let inputEl=null;

function log(s){ if(!termEl) return; termEl.textContent += s; termEl.scrollTop = termEl.scrollHeight; }

document.addEventListener('DOMContentLoaded', ()=>{
  termEl = document.getElementById('term');
  inputEl = document.getElementById('input');
  document.getElementById('connect').onclick = async ()=>{
    if(sock && sock.readyState===WebSocket.OPEN) sock.close();
    const proto = document.getElementById('proto').value;
    const host = document.getElementById('host').value.trim();
    const user = document.getElementById('user').value.trim();
    if(!host){ alert('Host required'); return; }
    sock = new WebSocket(wsUrl('/ws/terminal'));
    sock.onopen = ()=>{ sock.send(JSON.stringify({ cmd: proto, host, user })); log(`\n[connected ${proto} ${host}]\n`); };
    sock.onmessage = (ev)=>{ log(String(ev.data)); };
    sock.onclose = ()=>{ log("\n[disconnected]\n"); };
  };
  inputEl.addEventListener('keydown', (e)=>{
    if(e.key==='Enter' && sock && sock.readyState===WebSocket.OPEN){ sock.send(inputEl.value + "\n"); inputEl.value=''; }
  });
});

