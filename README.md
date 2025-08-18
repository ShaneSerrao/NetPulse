# PulsNet – Enterprise Network Monitoring & Device Manager

Modern, responsive ASP.NET Core app for monitoring SNMP-enabled devices (MikroTik focused), with live traffic, latency, device status, role-based access, and admin management.

- Backend: ASP.NET Core 8, EF Core, PostgreSQL, Identity
- Frontend: HTML5, CSS3, Vanilla JS (no extra packages)

## Documentation
- Deployment: `docs/DEPLOYMENT.md`
- Code Editing: `docs/CODE_EDITING.md`
- Style Editing: `docs/STYLE_EDITING.md`

## Quick Start (dev)
```
dotnet build
cd src/PulsNet.Web
ASPNETCORE_ENVIRONMENT=Development dotnet run
```
Visit http://localhost:5000 and log in after seeding.