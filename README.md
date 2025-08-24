# PulsNet (NetPuls_V0.7.6)

Modern, lightweight network monitoring and management web app.

## What’s included
- Auth (login/logout), cookie-based
- Dashboard with live device cards (latency, Mbps, usage), drag-and-drop order
- Devices CRUD, interface index, caps (down/up)
- MIBs: SNMP walk, catalog, attach OIDs, read values per device
- Management: bulk actions (apply template, run script, firmware, update config, rollback)
- Sites and Tenants management
- Users (SuperAdmin): create, set role, reset password
- Settings: Theme (mode/color), Polling, Global 2FA, Dashboard customization (versions), Device defaults (UI), Notifications (UI placeholders)
- Layout API: save per-user card order; versioned advanced layouts
- SSH/Telnet Web Terminal (Admin submenu: Terminal)

## Quick start
1. Ensure PostgreSQL and SNMP tools (`/usr/bin/snmpget`, `/usr/bin/snmpwalk`) are available.
2. Provide `config/secure_config.json` (copied to output by csproj) or set `PULSNET_SECRETS_PATH`.
3. Build and run:
   - `dotnet build`
   - `dotnet run`
4. On first run, a default admin is created:
   - username: `admin`
   - password: `admin123`

## Configuration (secure_config.json)
Case-insensitive JSON matching `ConfigService.AppConfig` (see `Services/ConfigService.cs`). Keys:
- `Database`: Postgres connection info
- `Security`: `{ Global2FAEnabled (bool), HttpsOnly (bool) }`
- `Polling`: `{ GlobalIntervalSeconds, CacheSeconds, OfflineThresholdSeconds }`
- `Theme`: `{ Name: dark|light, Primary, Accent }`
- `Smtp`: SMTP settings (reserved)

## Key pages
- `/index.html` Dashboard
- `/admin.html` Admin home (Devices list, Bulk Actions, Device Caps & Interface)
- `/devices.html` Separate Devices page (optional)
- `/management.html` Separate Management page (optional)
- `/mibs.html` SNMP Walk + Catalog/Attach
- `/sites.html` Sites
- `/tenants.html` Tenants
- `/users.html` Users + 2FA setup/verify/disable
- `/templates.html` Templates
- `/settings.html` Settings (Theme/Polling/Security, Layout versions, Device defaults, Notifications)
- `/terminal.html` SSH/Telnet web terminal

## Auth & 2FA
- Login: `POST /api/auth/login { username, password, totpCode? }`
- Logout: `POST /api/auth/logout`
- Me: `GET /api/auth/me`
- Global 2FA toggle: Settings page or API `/api/settings/global2fa`
- Per-user 2FA setup flow (SuperAdmin only):
  - `POST /api/users/{id}/2fa/setup` -> returns `{ secret, otpAuthUri }`
  - Scan in authenticator app (e.g., Google Authenticator)
  - `POST /api/users/{id}/2fa/verify { Code: "123456" }` -> enables 2FA for that user
  - `POST /api/users/{id}/2fa/disable`
- Enforcement on login: if global OR user 2FA is enabled, TOTP is required

## Devices & Live Metrics
- Devices: `GET/POST/PUT/DELETE /api/devices`
- Live metrics: `GET /api/devices/{id}/live` uses:
  - ifHCIn/OutOctets 64-bit counters sampled over 1s -> down/up Mbps
  - `interface_index` (set via `POST /api/devices/{id}/interface-index`)
  - ifHighSpeed (Mbps) when available for link capacity
  - Per-direction caps (down/up) to bound usage when enabled

## MIBs
- `POST /api/mibs/walk { ip, community, baseOid }` -> outputs lines
- `GET/POST /api/mibs/catalog`
- `POST /api/mibs/attach { deviceId, mibIds[] }`
- `GET /api/mibs/device/{deviceId}/oids`
- `GET /api/mibs/device/{deviceId}/values`

## Management
- `POST /api/management/apply-template { templateId, deviceIds[] }`
- `POST /api/management/run-script { scriptId, deviceIds[] }`
- `POST /api/management/firmware { firmwareVersion, deviceIds[] }`
- `POST /api/management/update-config { changeType, payload, deviceIds[] }`
- `POST /api/management/rollback { actionId }`
- `GET /api/management/{actionId}` -> status

## Settings
- Theme
  - `GET /api/settings/theme`
  - `POST /api/settings/theme { Name, Primary, Accent }`
  - Sun/Moon toggle writes `{ Name: "dark"|"light" }`
- Polling: `GET/POST /api/settings/polling`
- Global 2FA: `GET/POST /api/settings/global2fa`
- Layout
  - Card order: `GET/POST /api/layout/cards`
  - Advanced versions: `GET /api/layout/versions`, `POST /api/layout/versions { Name, Code }`, `POST /api/layout/versions/delete { Name }`

## Terminal (SSH/Telnet)
- WebSocket endpoint: `ws(s)://host/ws/terminal`
  - First message must be text JSON: `{ cmd: "ssh"|"telnet", host:"ip|dns", user?:"username" }`
  - Text messages are written to stdin; stdout/stderr streamed back
  - Auth required (User/Operator/Admin/SuperAdmin)
- UI: `/terminal.html`

## Theme
- CSS variables in `wwwroot/styles.css`; `.light-theme` class toggled on `<html>` for light mode
- Theme toggle in navbar persists setting via `/api/settings/theme`

## Known constraints
- SNMP commands used: `/usr/bin/snmpget` & `/usr/bin/snmpwalk`
- Terminal uses system `/usr/bin/ssh` and `/usr/bin/telnet`
- No external packages are installed beyond those already in project

## Development
- Code style: clear names, early returns, minimal nesting
- Security: cookie auth; basic headers; rate-limiter; role checks on sensitive endpoints
- UI: vanilla HTML/CSS/JS, no TypeScript, no build tools

## Versioning
- Current version: `PulsNet_V0.7.6`
- Tag format: `PulsNet_Vx.y.z`

## Changelog highlights (since V0.7.5)
- Accurate live metrics (64-bit counters, interface index, caps, ifHighSpeed)
- Admin submenu and layout cleanups; MIBs page repaired
- Device Caps & Interface UI; Settings expanded with layout versions
- Sun/Moon theme toggle with persistence
- Full 2FA flow (setup/verify/disable, enforced on login)
- SSH/Telnet terminal (WebSocket + UI)