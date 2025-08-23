# PulsNet – Implemented Features (Rebuilt V0.7+)
- Roles: SuperAdmin/Admin/Operator/Viewer; cookie auth; brute-force lockout; optional 2FA (off by default).
- DB: base + v0.7 extensions (tenants, templates/scripts, action queue, config history, MIB catalog, sites, caps).
- Monitoring: ping + SNMP ifHCIn/Out; 64-bit deltas; per-device interface index and caps; caching.
- Device Management: queue (apply template/script/firmware/update config), staged rollout, audit (history).
- MIBs: snmpwalk on server; categorize; add to catalog; attach OIDs per device for overlays.
- Sites/Map: CRUD sites (address, lat, lon); assign devices; pin-drop UI (frontend map next step).
- Settings: dark/light mode; primary/accent; polling; offline threshold; layout default toggle.
- Dashboard: cards with IP blur/reveal, usage bar, click overlay hook, drag-and-drop (next step persist).
