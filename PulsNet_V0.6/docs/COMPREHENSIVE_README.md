# PulsNet – Enterprise Network Monitoring & Device Manager (Full Guide)

A modern ASP.NET Core 8 app that monitors SNMP-enabled devices (MikroTik-focused), shows live traffic, latency, device status, and provides an Admin panel for device management and feature toggles like 2FA. Frontend uses HTML/CSS/Vanilla JS only.

## Contents
- Overview
- System Requirements
- Directory Structure (with absolute paths)
- Installation (Debian 12)
- Database Setup (PostgreSQL)
- Secrets Configuration
- Build & Run (Dev / Prod)
- Systemd Service
- Reverse Proxy (Nginx + HTTPS)
- Usage Guide (Dashboard, Admin, Settings)
- Security & Hardening
- Backups & Restore
- Testing
- Troubleshooting
- Roadmap

## Overview
- Backend: ASP.NET Core 8, EF Core 8, PostgreSQL, ASP.NET Identity (2FA disabled by default)
- Frontend: HTML5, CSS3, Vanilla JS (no packages)
- Monitoring: background polling with ping + SNMP hooks, memory cache
- Single secure secrets file with DB credentials, global settings
- Roles: Admin (full), User (monitor-only)

## System Requirements
- Debian 12 (Bookworm)
- .NET SDK 8.0 and ASP.NET Core Runtime 8.0
- PostgreSQL 15+
- net-snmp (`snmpget`)
- Nginx (optional for HTTPS)

## Directory Structure (absolute paths)
Assuming the project resides at `/opt/pulsnet` in production and `/workspace` in development.

- Solution: `/opt/pulsnet/PulsNet.sln`
- Web app project: `/opt/pulsnet/src/PulsNet.Web`
- Entrypoint: `/opt/pulsnet/src/PulsNet.Web/Program.cs`
- Config loader: `/opt/pulsnet/src/PulsNet.Web/config/SecretsConfig.cs`
- EF DbContext: `/opt/pulsnet/src/PulsNet.Web/Data/AppDbContext.cs`
- Seed roles/admin/settings: `/opt/pulsnet/src/PulsNet.Web/Data/SeedData.cs`
- Models: `/opt/pulsnet/src/PulsNet.Web/Models/{ApplicationUser,AppSettings,Device,TrafficSample}.cs`
- Services:
  - Monitoring: `/opt/pulsnet/src/PulsNet.Web/Services/MonitoringService.cs`
  - SNMP: `/opt/pulsnet/src/PulsNet.Web/Services/Snmp/SnmpClient.cs`
- Controllers: `/opt/pulsnet/src/PulsNet.Web/Controllers/{Account,Admin,Dashboard}Controller.cs`
- Views:
  - Layout: `/opt/pulsnet/src/PulsNet.Web/Views/Shared/_Layout.cshtml`
  - Dashboard: `/opt/pulsnet/src/PulsNet.Web/Views/Dashboard/Index.cshtml`
  - Admin: `/opt/pulsnet/src/PulsNet.Web/Views/Admin/{Devices,CreateDevice,EditDevice,Settings}.cshtml`
  - Account: `/opt/pulsnet/src/PulsNet.Web/Views/Account/Login.cshtml`
- Static: `/opt/pulsnet/src/PulsNet.Web/wwwroot/{css/site.css,js/dashboard.js}`
- Docs: `/opt/pulsnet/docs/COMPREHENSIVE_README.md`
- Secrets: `/etc/pulsnet/pulsnet.secrets.json`
- Systemd service: `/etc/systemd/system/pulsnet.service`

Development paths mirror `/workspace` instead of `/opt/pulsnet`.

## Installation (Debian 12)
```bash
sudo wget -q https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb
sudo dpkg -i /tmp/packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0 aspnetcore-runtime-8.0 postgresql postgresql-contrib snmp
```

## Database Setup
```bash
sudo systemctl enable --now postgresql
sudo -u postgres psql <<'SQL'
CREATE DATABASE pulsnet;
CREATE USER pulsnet WITH ENCRYPTED PASSWORD 'changeme';
GRANT ALL PRIVILEGES ON DATABASE pulsnet TO pulsnet;
SQL
```

## Secrets Configuration
Create `/etc/pulsnet/pulsnet.secrets.json`:
```bash
sudo mkdir -p /etc/pulsnet
sudo tee /etc/pulsnet/pulsnet.secrets.json >/dev/null <<'JSON'
{
  "db": {
    "host": "localhost",
    "port": 5432,
    "database": "pulsnet",
    "username": "pulsnet",
    "password": "changeme"
  },
  "helpdeskEmail": "helpdesk@example.com",
  "globalTwoFactorEnabled": false,
  "globalPollIntervalSeconds": 5
}
JSON
sudo chmod 640 /etc/pulsnet/pulsnet.secrets.json
sudo chown root:root /etc/pulsnet/pulsnet.secrets.json
```

## Build & Run (Development)
```bash
cd /workspace
dotnet build
# First-time EF migrations
dotnet tool install --global dotnet-ef || true
export PATH="$HOME/.dotnet/tools:$PATH"
cd /workspace/src/PulsNet.Web
dotnet ef migrations add InitialCreate
dotnet ef database update
ASPNETCORE_ENVIRONMENT=Development dotnet run
```
- Access http://localhost:5000
- Login: `admin@pulsnet.local` / `PulsNet#2025`
- 2FA is disabled globally and in the user by default. You can enable it later via Settings.

## Build & Run (Production)
```bash
sudo mkdir -p /opt/pulsnet
sudo rsync -a /workspace/ /opt/pulsnet/
cd /opt/pulsnet
dotnet build
cd /opt/pulsnet/src/PulsNet.Web
export PATH="$HOME/.dotnet/tools:$PATH"
dotnet tool install --global dotnet-ef || true
dotnet ef database update
```

## Systemd Service
`/etc/systemd/system/pulsnet.service`:
```ini
[Unit]
Description=PulsNet Monitoring Web
After=network.target

[Service]
WorkingDirectory=/opt/pulsnet/src/PulsNet.Web
ExecStart=/usr/bin/dotnet PulsNet.Web.dll
Restart=always
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000
Environment=PULSNET_SECRETS_PATH=/etc/pulsnet/pulsnet.secrets.json
User=www-data
Group=www-data

[Install]
WantedBy=multi-user.target
```
Enable:
```bash
sudo systemctl daemon-reload
sudo systemctl enable --now pulsnet
```

## Reverse Proxy (Nginx + HTTPS)
```nginx
server {
  listen 80;
  server_name your.domain;
  location / {
    proxy_pass http://127.0.0.1:5000;
    proxy_set_header Host $host;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
  }
}
```
```bash
sudo ln -s /etc/nginx/sites-available/pulsnet /etc/nginx/sites-enabled/pulsnet
sudo nginx -t && sudo systemctl reload nginx
```
Use certbot to enable HTTPS.

## Usage Guide
- Login: `/Account/Login`
- Dashboard: `/Dashboard`
  - Live cards show down/up Mbps, latency, online status, link usage; IPs are blurred by default and reveal for 15s.
- Admin: `/Admin/Devices` (CRUD for devices)
- Settings: `/Admin/Settings`
  - Global Two-Factor Enabled: toggle on/off site-wide 2FA requirement (default OFF)
  - Global Poll Interval: set background polling interval in seconds
  - Theme colors: adjust primary/accent colors and theme name

## Security & Hardening
- Change default admin password on first login
- Enforce HTTPS via reverse proxy
- Limit DB access; use strong DB password
- Keep secrets file permissions strict (640, root-owned)
- Configure firewall to allow only needed ports (80/443)

## Backups
- Backup database:
```bash
sudo -u postgres pg_dump -Fc pulsnet > /var/backups/pulsnet_$(date +%F).dump
```
- Restore:
```bash
sudo -u postgres pg_restore -d pulsnet /var/backups/pulsnet_YYYY-MM-DD.dump
```

## Testing
```bash
cd /opt/pulsnet
dotnet test PulsNet.sln
```

## Troubleshooting
- Service logs: `sudo journalctl -u pulsnet -f`
- Check .NET: `dotnet --info`
- Verify secrets: ensure `/etc/pulsnet/pulsnet.secrets.json` exists
- DB connectivity: `psql "host=localhost dbname=pulsnet user=pulsnet"`
- SNMP reachability: `snmpget -v2c -c COMMUNITY DEVICE_IP sysUpTime.0`

## Roadmap
- Implement SNMP counters to Mbps conversion and MikroTik OIDs
- Offline devices page + email notifications
- Per-client interval override auto-expiry UI
- Optional SMS/email alerts for critical failures
- PWA support (manifest + service worker)