### PulsNet – Deployment (Debian 12)

#### Prerequisites
- Debian 12 (bookworm)
- Sudo privileges
- Internet access

#### Install .NET 8 Runtime and SDK
```bash
sudo apt update && sudo apt install -y wget apt-transport-https gnupg
wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update && sudo apt install -y dotnet-sdk-8.0 aspnetcore-runtime-8.0
```

#### Install PostgreSQL and net-snmp
```bash
sudo apt install -y postgresql postgresql-contrib snmp snmpd
```

#### Database setup
```bash
sudo -u postgres psql -c "CREATE USER pulsnet_user WITH PASSWORD 'change_me';"
sudo -u postgres psql -c "CREATE DATABASE pulsnet OWNER pulsnet_user;"
sudo -u postgres psql -d pulsnet -f /workspace/db/schema_postgres.sql
```

Set admin password securely: change immediately after first login. A default admin is created at first run (admin/admin123).

Edit `/workspace/config/secure_config.json` with your DB and SMTP credentials.

#### Build and run
```bash
cd /workspace/src/PulsNet
dotnet build -c Release
ASPNETCORE_URLS="http://0.0.0.0:8080" dotnet run -c Release
```

Visit: `http://server:8080/login.html`

#### Systemd service (optional)
Create `/etc/systemd/system/pulsnet.service`:
```ini
[Unit]
Description=PulsNet Service
After=network.target

[Service]
WorkingDirectory=/workspace/src/PulsNet
ExecStart=/usr/bin/dotnet /workspace/src/PulsNet/bin/Release/net8.0/PulsNet.dll
Restart=always
Environment=ASPNETCORE_URLS=http://0.0.0.0:8080
User=www-data
Group=www-data

[Install]
WantedBy=multi-user.target
```
```bash
sudo systemctl daemon-reload
sudo systemctl enable --now pulsnet
```

#### File/folder permissions
- `config/secure_config.json`: readable by app user only.
```bash
sudo chown www-data:www-data /workspace/config/secure_config.json
sudo chmod 600 /workspace/config/secure_config.json
```

#### Backup/restore
```bash
# Backup
pg_dump -U pulsnet_user -h 127.0.0.1 -Fc pulsnet > pulsnet_$(date +%F).dump
# Restore
pg_restore -U pulsnet_user -h 127.0.0.1 -d pulsnet pulsnet_YYYY-MM-DD.dump
```

#### HTTPS
Put Nginx in front and terminate TLS or configure Kestrel with certs. Enforce HTTPS via proxy.

