# PulsNet Code Editing Guide

## Key Projects
- `src/PulsNet.Web`: ASP.NET Core MVC app (controllers, views, services)
- `tests/PulsNet.Tests`: Unit tests

## Configuration
- `config/SecretsConfig.cs`: Loads secrets from `/etc/pulsnet/pulsnet.secrets.json` or `PULSNET_SECRETS_PATH`.
- `Program.cs`: Registers services, DbContext, Identity, background workers.

## Data Layer
- `Data/AppDbContext.cs`: EF Core DbContext with Identity + app entities.
- `Models/Device.cs`, `Models/TrafficSample.cs`, `Models/ApplicationUser.cs`.
- Migrations: create with `dotnet ef migrations add <Name>` (install `dotnet-ef` tool if needed).

## Services
- `Services/Snmp/SnmpClient.cs`: Wraps net-snmp commands. Extend with specific MikroTik OIDs.
- `Services/MonitoringService.cs`: Background polling loop. Adjust intervals and add caching logic.

## Web Layer
- `Controllers/DashboardController.cs`: Dashboard and JSON endpoints.
- `Controllers/AdminController.cs`: Admin CRUD for devices.
- `Controllers/AccountController.cs`: Login/Logout.
- Views under `Views/*` and static assets under `wwwroot/*`.

## Adding Per-Client Intervals
- Extend `Device` with interval fields (already present) and update `MonitoringService` to apply per-device overrides with expiry (5 minutes) before falling back to global.

## Security
- All input is validated server-side via model validation. Strengthen as needed.
- Cookie auth configured in `Program.cs`. Update policies/roles in controllers.

## SNMP OIDs
Add constants and parsing in `SnmpClient` and `MonitoringService` to collect:
- Interface speeds and counters
- CPU, memory, etc. (optional)

## Testing
- Add unit tests for Db operations and controller endpoints in `tests/PulsNet.Tests`.