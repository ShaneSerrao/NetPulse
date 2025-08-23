# How to Use (Rebuilt V0.7+)
1) Apply DB: /workspace/rebuilt/db/schema_postgres.sql then schema_v07_extensions.sql
2) Configure /workspace/rebuilt/config/secure_config.json
3) Build & run:
   cd /workspace/rebuilt/src/PulsNet
   dotnet build
   ASPNETCORE_URLS="http://0.0.0.0:8080" dotnet run
4) Use:
   - /login.html for login
   - / for dashboard (drag-drop coming next)
   - /admin.html for theme/polling and device caps/interface
   - /mibs.html to run snmpwalk and manage catalog
   - /sites.html to create sites and coordinates
5) Accuracy:
   - Set correct interface index and caps; Down=ifHCIn, Up=ifHCOut
