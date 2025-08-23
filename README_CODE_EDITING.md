# PulsNet – Code Editing

## Structure
- src/PulsNet: ASP.NET Core app (Program.cs, Controllers, Services, Data, wwwroot)
- config/secure_config.json: single source for DB, SMTP, polling, theme
- db/schema_postgres.sql: base schema
- db/schema_v07_extensions.sql: v0.7+ tables

## Guidelines
- Add core logic in Services; keep Controllers thin.
- Validate inputs (IPs, VLAN IDs, queue names).
- Use async with CancellationToken.
- Return only necessary fields (never SNMP secrets).
- Background work: use hosted services.

## SNMP Notes
- Uses net-snmp tools (snmpget/snmpwalk) via Process.
- Cache results with IMemoryCache to reduce load.
- Ensure interface_index matches WAN interface for accurate rates.

## Extending
- Device actions: add new action type in DeviceManagementService and ActionProcessor.
- MIBs: update catalog and categories; add parsing if needed.
- Tenants/Sites: extend CRUD endpoints and UI pages accordingly.
