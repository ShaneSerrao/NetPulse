### Smoke Tests

1. Health
   - GET `/api/health` returns `{ status: "ok" }`.
2. Auth
   - POST `/api/auth/login` with admin/admin123 returns 200.
   - GET `/api/auth/me` returns username and role.
3. Devices
   - POST `/api/devices` with Admin role creates a device.
   - GET `/api/devices` returns list.
   - GET `/api/devices/{id}/live` returns live stats.
4. Offline
   - GET `/api/offline` lists offline devices.
5. Settings
   - GET `/api/settings/theme` returns theme.
   - POST `/api/settings/theme` updates theme.

