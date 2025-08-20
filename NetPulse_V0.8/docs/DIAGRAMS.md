# PulsNet Diagrams

## High-Level Architecture
```
[Users/Browser] --HTTPS--> [ASP.NET MVC/UI + Controllers] --DI--> [Services]
                                      |                              |
                                      v                              v
                                 [Identity]                  [Polling/Workers]
                                      |                              |
                                      v                              v
                                   [DB: PostgreSQL/MySQL]   [Integrations]
```

## GUI Sitemap
```
Dashboard -> Tenant groups -> Devices -> Metrics & Actions
Devices -> List/Detail/Interfaces/Inventory/Discovery
MIBs -> Device-centric MIB/OIDs -> Assign to Poll Jobs
Monitoring -> Metrics/Thresholds/Notifications/History
Deployment -> Configs/Backups/Firmware/Scripts/Bulk
Tenants -> CRUD + Assign Users/Devices/Channels
Users -> CRUD + Roles + Feature Flags + Device Assignments
Admin/Settings -> Theme/Site/Integrations/Polling/Retention/Licensing
```

## ER Model (Simplified)
```
Tenant--<Device--<Interface
Tenant--<User--<UserRoles>--Role
Device--<TrafficSample, HealthSample, Incident
Device--<DeviceMib>--Mib--<MibOid
Device--<PollJob--<PollMetricAssignment>--MibOid
Device--<ThresholdRule
Tenant--<NotificationChannel
Device--<Backup, ConfigAssignment
```

## Export to PDF
- Use `pandoc` or `wkhtmltopdf` locally:
```
pandoc -o whitepaper.pdf WHITEPAPER.md
pandoc -o diagrams.pdf DIAGRAMS.md
```
- Or with Docker (optional):
```
docker run --rm -v "$PWD/docs":/data pandoc/core:3.1 -o /data/whitepaper.pdf /data/WHITEPAPER.md
```