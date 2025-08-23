# PulsNet – How to Use (V0.7+)

## Database
1) Base schema:
   psql -d pulsnet -f db/schema_postgres.sql
2) v0.7 extensions:
   psql -d pulsnet -f db/schema_v07_extensions.sql

## Login
- Default: admin / admin123 (change immediately). Global 2FA is off by default (toggle in Settings).

## Settings
- Theme: set colors, toggle Dark/Light. Saved to DB and applied across all pages.
- Polling: set global interval, cache seconds, offline threshold.
- Layout: “Reset to default layout” restores the stock card layout and options.
- Cards: drag to reorder; choose compact/detailed; toggle visibility of fields; per-card reset available.

## Devices
- Add IP, SNMP community, Max Link, Interface Index (WAN interface).
- Per-device caps: enable and set realistic Down/Up caps (Mbps) to clamp spikes and avoid identical Up/Down readings.

## Dashboard
- Animated link-usage; masked IP (Reveal → auto mask after 15s).
- Click any device card → overlay with selected OID metrics from the MIBs module.

## Offline
- Lists devices currently offline. Email alert triggers on state changes from online to offline.

## MIBs
- Run snmpwalk (base OID + target IP). Results categorized; add to catalog.
- Select OIDs and attach to devices; device overlay displays the chosen metrics.

## Device Management (SuperAdmin/Admin)
- Actions: ApplyTemplate, RunScript, FirmwareUpdate (staged), UpdateInterface/VLAN/Queue/VPN, ValidateChanges, Rollback.
- Confirm actions; track progress live; rollback available.
- All actions are queued and audited in ConfigHistory.

## Tenants
- Create tenants, assign devices; filter dashboard and bulk ops by tenant.

## Sites & Map
- Create sites (address, lat, lon). Use pin-drop to create/update.
- Assign devices to sites; filter by site/tenant.

## Accuracy Tips
- Set the correct interface_index for each device.
- Enable per-device caps for realistic maximum rates.
- ifHCInOctets → Down; ifHCOutOctets → Up (separate calculations, no mirroring).
