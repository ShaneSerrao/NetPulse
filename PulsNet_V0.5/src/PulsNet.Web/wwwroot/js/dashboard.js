(function(){
  const cards = document.querySelectorAll('.device-card');
  const GLOBAL_INTERVAL_MS = 3000;

  function setStatus(card, isOnline) {
    const dot = card.querySelector('.status .dot');
    const text = card.querySelector('.status .text');
    if (isOnline) {
      dot.classList.remove('offline');
      dot.classList.add('online');
      text.textContent = 'Online';
    } else {
      dot.classList.remove('online');
      dot.classList.add('offline');
      text.textContent = 'Offline';
    }
  }

  function updateMetrics(card, sample){
    card.querySelector('.down').textContent = (sample.downloadMbps || 0).toFixed(2);
    card.querySelector('.up').textContent = (sample.uploadMbps || 0).toFixed(2);
    card.querySelector('.latency').textContent = sample.latencyMs >= 0 ? sample.latencyMs : '—';
    setStatus(card, !!sample.isOnline);
    const max = parseFloat(card.dataset.maxMbps || '1000');
    const usage = Math.min(100, Math.round(100 * (sample.downloadMbps || 0) / max));
    const fill = card.querySelector('.link-usage .fill');
    fill.style.width = usage + '%';
    fill.style.background = `linear-gradient(90deg, rgb(${Math.min(255, usage*2.55)}, ${Math.max(0, 255-usage*2.55)}, 80), #333)`;
  }

  async function fetchSample(id){
    const res = await fetch(`/Dashboard/LiveSample?id=${id}`, { cache: 'no-store' });
    if (!res.ok) throw new Error('fetch fail');
    return await res.json();
  }

  function setupReveal(card){
    const btn = card.querySelector('.reveal');
    const span = card.querySelector('.ip');
    let timer;
    btn.addEventListener('click', () => {
      span.textContent = span.dataset.ip;
      span.classList.add('revealed');
      clearTimeout(timer);
      timer = setTimeout(() => {
        span.textContent = '•••.•••.•••.•••';
        span.classList.remove('revealed');
      }, 15000);
    });
  }

  function loop(){
    cards.forEach(async card => {
      const id = card.dataset.deviceId;
      try {
        const sample = await fetchSample(id);
        updateMetrics(card, sample);
      } catch {}
    });
  }

  cards.forEach(c => setupReveal(c));
  setInterval(loop, GLOBAL_INTERVAL_MS);
  loop();
})();