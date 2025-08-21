# PulsNet White Paper (Enterprise MikroTik Monitoring & Deployment)

## Executive Summary
PulsNet is an ASP.NET Core–based, multi-tenant network observability and deployment platform focused on MikroTik environments. It delivers live monitoring (SNMP, RouterOS API), discovery (SNMP/LLDP/CDP/subnet), historical analytics, NetFlow/sFlow insight, device configuration backups, versioning with rollback, firmware orchestration, and automated provisioning (ZTP). A responsive, role-based GUI enables end-to-end management without coding.

## Architecture Overview
- Web UI: Razor views + Vanilla JS (responsive, micro-animations)
- Services: Background workers for polling, discovery, backups, firmware, NetFlow
- Integrations: Email/Telegram/Slack/SMS/Webhooks
- Persistence: PostgreSQL/MySQL (configurable via a single secrets file)
- Security: ASP.NET Identity (Admin/Operator/Viewer + custom), cookie auth, idle timeouts, optional 2FA
- Scalability: Async polling, cached latest metrics, rollup windows, retention policies

## Monitoring Capabilities
- Device health: CPU, RAM, disk, temperatures, uptime, reboots
- Interfaces: Real-time + historical traffic, utilization, errors/drops
- Reachability: Ping, latency, jitter, packet loss, online/offline
- Inventory: Router model, RouterOS, firmware, serial, license, interfaces
- Users/Clients: DHCP/PPPoE/Hotspot sessions; per-client usage
- NetFlow/sFlow: top talkers, protocol/app, per-tenant/device aggregation

## Deployment & Automation
- ZTP: Detect & auto-configure new routers
- Configuration management: Templates, variables, versioning, diffs, rollback, device/tenant assignment
- Backups: Scheduled config backups, view/restore across versions
- Firmware orchestration: Catalog, scheduled upgrades, staged rollout, auto-rollback on failure
- Scripts & bulk ops: RouterOS scripts, interface/firewall/queues/VLAN/VPN bulk changes

## RBAC & Multi-Tenancy
- Tenants: partitioned visibility, notification channels
- Roles: SuperAdmin (incl. licensing), Admin, Operator, Viewer + custom permissions
- Device assignments: user-specific access per device

## Reliability & Security
- Optional 2FA, brute force lockout, secure cookies, HTTPS
- Secrets from a single config file, no hard-coded credentials
- Input sanitization for SNMP, IPs, OIDs; principle of least privilege
- Logs & Incidents: device events, thresholds, notifications

## Operations & Extensibility
- Configurable polling intervals; data retention and purges
- Theming and layout via Admin Settings
- MIB/OID catalogs and custom metrics
- Modular services; add new workers and controllers safely

## Deployment
- Debian 12: .NET 8 runtime, PostgreSQL/MySQL, net-snmp
- Systemd service; Nginx TLS reverse proxy
- Backups via pg_dump/mysqldump; restore procedures

## Roadmap
- Advanced topology rendering; richer RouterOS API modules
- Enhanced anomaly detection with baselines; SLO/SLAs
- Built-in reporting engine; scheduled PDFs