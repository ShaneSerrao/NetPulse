(function() {
    const cards = document.querySelectorAll('.live-device-card');  // Only select live cards
    const GLOBAL_INTERVAL_MS = 3000;

    // Update device status (Online/Offline)
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

    // Update device metrics like download/upload speed, latency, etc.
    function updateMetrics(card, sample) {
        // Update all metrics inside the card
        card.querySelector('.down').textContent = (sample.downloadMbps || 0).toFixed(2);
        card.querySelector('.up').textContent = (sample.uploadMbps || 0).toFixed(2);
        card.querySelector('.latency').textContent = sample.latencyMs >= 0 ? sample.latencyMs : '—';
        setStatus(card, !!sample.isOnline);  // Set Online/Offline status
        const max = parseFloat(card.dataset.maxMbps || '1000');
        const usage = Math.min(100, Math.round(100 * (sample.downloadMbps || 0) / max));
        const fill = card.querySelector('.link-usage .fill');
        fill.style.width = usage + '%';
        fill.style.background = `linear-gradient(90deg, rgb(${Math.min(255, usage * 2.55)}, ${Math.max(0, 255 - usage * 2.55)}, 80), #333)`;
    }

    // Fetch the live sample data from the server
    async function fetchSample(id) {
        const res = await fetch(`/Dashboard/LiveSample?id=${id}`, { cache: 'no-store' });
        if (!res.ok) throw new Error('fetch fail');
        const data = await res.json();
        return data;
    }

    // Setup reveal functionality (for showing IP)
    function setupReveal(card) {
        const btn = card.querySelector('.reveal');
        const span = card.querySelector('.ip');
        if (!btn) {
            console.warn('Skipping card without .reveal:', card);
            return;  // Skip cards without a reveal button
        }
        let timer;
        btn.addEventListener('click', () => {
            span.textContent = span.dataset.ip;
            span.classList.add('revealed');
            clearTimeout(timer);
            timer = setTimeout(() => {
                span.textContent = '•••.•••.•••.•••';
                span.classList.remove('revealed');
            }, 15000);  // Reset after 15 seconds
        });
    }

    // Main function to loop through each device card and update metrics
    function loop() {
        cards.forEach(async card => {
            const id = card.dataset.deviceId;
            if (!id) {
                console.warn('Skipping card without data-device-id:', card); // Skip cards missing device ID
                return;
            }
            try {
                const sample = await fetchSample(id);
                updateMetrics(card, sample);  // Update the device metrics
            } catch (err) {
                console.error(`Error fetching sample for device ${id}:`, err);
            }
        });
    }

    // Initialize the page
    cards.forEach(c => setupReveal(c));  // Setup the IP reveal for each card

    // Set an interval to refresh data periodically
    setInterval(loop, GLOBAL_INTERVAL_MS);
    loop();  // Initial call to fetch data immediately
})();
