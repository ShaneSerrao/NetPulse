# Deployment (Debian 12)
- Install .NET 8, PostgreSQL, net-snmp. Fix pg_hba.conf: peer local, md5 on 127.0.0.1/::1, restart.
- Create DB user/db idempotently; apply base and v0.7 schemas.
- Set secure_config.json with DB creds; chmod 600, restrictive ownership.
- Run Kestrel on 0.0.0.0:8080 behind Nginx for HTTPS.
