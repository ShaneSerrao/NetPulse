# PulsNet Deployment (Debian 12)

## Prerequisites
- Debian 12 (Bookworm)
- sudo privileges
- Internet connectivity

## Install .NET 8 SDK and ASP.NET Runtime
```
sudo wget -q https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb
sudo dpkg -i /tmp/packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0 aspnetcore-runtime-8.0
```

## Install PostgreSQL
```
sudo apt-get install -y postgresql postgresql-contrib
sudo systemctl enable --now postgresql
```

Create database and user:
```
sudo -u postgres psql <<'SQL'
CREATE DATABASE pulsnet;
CREATE USER pulsnet WITH ENCRYPTED PASSWORD 'changeme';
GRANT ALL PRIVILEGES ON DATABASE pulsnet TO pulsnet;
SQL
```

## Install net-snmp (for SNMP polling)
```
sudo apt-get install -y snmp
```

## Configure Secrets File
Create `/etc/pulsnet/pulsnet.secrets.json` and secure permissions:
```
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

## Build and Run
From the project root:
```
dotnet build
cd src/PulsNet.Web
dotnet ef database update || dotnet run --no-build
```
Default admin user is `admin@pulsnet.local` with password `PulsNet#2025` (change after first login).

## Systemd Service (optional)
Create `/etc/systemd/system/pulsnet.service`:
```
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
```
sudo systemctl daemon-reload
sudo systemctl enable --now pulsnet
```