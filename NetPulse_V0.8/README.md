# PulsNet – Enterprise Network Monitoring & Device Manager

A modern ASP.NET Core 8 web app to monitor SNMP-enabled devices (MikroTik-focused). 2FA is disabled by default. Enable/disable via Admin > Settings.

- Solution: `/workspace/PulsNet.sln`
- Web app: `/workspace/src/PulsNet.Web`
- Comprehensive guide: `docs/COMPREHENSIVE_README.md`

Quick start (dev):
```
cd /workspace
dotnet build
cd src/PulsNet.Web
dotnet tool install --global dotnet-ef || true
export PATH="$HOME/.dotnet/tools:$PATH"
dotnet ef migrations add InitialCreate
dotnet ef database update
ASPNETCORE_ENVIRONMENT=Development dotnet run
```
Login: `admin@pulsnet.local` / `PulsNet#2025`