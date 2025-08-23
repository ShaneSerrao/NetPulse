# PulsNet – Deployment (Debian 12)

## Prereqs
- Debian 12 (bookworm)
- .NET 8 SDK + ASP.NET runtime (from Microsoft packages)
- PostgreSQL 14+ and net-snmp tools

## DB setup
sudo -u postgres psql -c "CREATE USER pulsnet_user WITH PASSWORD 'change_me';"
sudo -u postgres psql -c "CREATE DATABASE pulsnet OWNER pulsnet_user;"
sudo -u postgres psql -d pulsnet -f db/schema_postgres.sql
sudo -u postgres psql -d pulsnet -f db/schema_v07_extensions.sql

## Config
- Edit config/secure_config.json with DB and SMTP settings.
- Ensure correct permissions:
  chown www-data:www-data config/secure_config.json
  chmod 600 config/secure_config.json

## Build & Run
cd src/PulsNet
dotnet build -c Release
ASPNETCORE_URLS="http://0.0.0.0:8080" dotnet run -c Release
Visit: http://server:8080/login.html

## Systemd (optional)
Create /etc/systemd/system/pulsnet.service:
[Unit]
Description=PulsNet Service
After=network.target

[Service]
WorkingDirectory=/path/to/repo/src/PulsNet
ExecStart=/usr/bin/dotnet /path/to/repo/src/PulsNet/bin/Release/net8.0/PulsNet.dll
Restart=always
Environment=ASPNETCORE_URLS=http://0.0.0.0:8080
User=www-data
Group=www-data

[Install]
WantedBy=multi-user.target

sudo systemctl daemon-reload
sudo systemctl enable --now pulsnet
