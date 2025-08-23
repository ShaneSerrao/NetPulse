// Helper fetch function
async function j(u, o = {}) {
    const r = await fetch(u, Object.assign({ headers: { 'Content-Type': 'application/json' }, credentials: 'include' }, o));
    if (!r.ok) throw 0;
    return r.json();
}

// Load site list
async function load() {
    const list = await j('/api/sites');
    const div = document.getElementById('list');
    div.innerHTML = '';
    for (const s of list) {
        const row = document.createElement('div');
        row.className = 'row space';
        row.innerHTML = `<div>${s.name} (${s.latitude}, ${s.longitude})</div>`;
        div.appendChild(row);
    }
}

// Save / update site
const nameEl = name, addrEl = addr, latEl = lat, lonEl = lon;
document.getElementById('save').onclick = async () => {
    await j('/api/sites', {
        method: 'POST',
        body: JSON.stringify({
            name: nameEl.value.trim(),
            address: addrEl.value.trim() || null,
            latitude: latEl.value ? +latEl.value : null,
            longitude: lonEl.value ? +lonEl.value : null
        })
    });
    await load();
};

// Simple Web Mercator conversion
function lon2x(lon, z) { return (lon + 180) / 360 * 256 * Math.pow(2, z); }
function lat2y(lat, z) { const r = Math.log(Math.tan((lat + 90) * Math.PI / 360)); return (1 - r / Math.PI) / 2 * 256 * Math.pow(2, z); }
function x2lon(x, z) { return x / (256 * Math.pow(2, z)) * 360 - 180; }
function y2lat(y, z) { const n = Math.PI - 2 * Math.PI * y / (256 * Math.pow(2, z)); return (180 / Math.PI * Math.atan(0.5 * (Math.exp(n) - Math.exp(-n)))); }

const z = 3, center = { lat: 0, lon: 0 };

// Draw simple placeholder map
function drawMap() {
    const c = document.getElementById('map');
    const ctx = c.getContext('2d');
    ctx.fillStyle = '#333';
    ctx.fillRect(0, 0, c.width, c.height);
    // Optionally draw center cross
    ctx.strokeStyle = '#fff';
    ctx.beginPath();
    ctx.moveTo(c.width / 2, 0);
    ctx.lineTo(c.width / 2, c.height);
    ctx.moveTo(0, c.height / 2);
    ctx.lineTo(c.width, c.height / 2);
    ctx.stroke();
}

// Handle click to set lat/lon
const map = document.getElementById('map');
map.addEventListener('click', (e) => {
    const rect = map.getBoundingClientRect();
    const px = e.clientX - rect.left;
    const py = e.clientY - rect.top;
    // Naive mapping assuming center 0,0
    const lon = (px / map.width) * 360 - 180;
    const lat = 90 - (py / map.height) * 180;
    latEl.value = lat.toFixed(6);
    lonEl.value = lon.toFixed(6);
});

// Initialize map and load sites
window.addEventListener('DOMContentLoaded', () => {
    drawMap();
    load();
});
