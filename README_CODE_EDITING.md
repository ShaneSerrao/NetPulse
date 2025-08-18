### PulsNet – Code Editing Guide

#### Structure
- `src/PulsNet/Program.cs`: App wiring, services registration, middleware, endpoints.
- `src/PulsNet/Controllers/*`: REST APIs.
- `src/PulsNet/Services/*`: Business logic (auth, devices, monitoring, polling, email, settings).
- `src/PulsNet/Data/Db.cs`: Lightweight DB helper using Npgsql.
- `src/PulsNet/wwwroot/*`: Frontend HTML, CSS, JS (vanilla).
- `config/secure_config.json`: Single source of truth for DB and base settings.
- `db/schema_postgres.sql`: Database schema.

#### Editing backend logic
1. Add methods in a `Service` first, keep controllers thin.
2. Validate inputs early (IPs, SNMP community strings, integers ranges).
3. Prefer async APIs and cancellation tokens.
4. Log errors with context; avoid catching without handling.
5. Keep authentication and authorization via roles on controllers/policies.

#### Adding a new endpoint
1. Create a method in a controller under `Controllers/`.
2. Inject required service via constructor.
3. Annotate with `[Authorize]` or `[Authorize(Policy="AdminOnly")]`.
4. Return DTOs with only necessary fields (avoid secrets like SNMP community).

#### SNMP notes
- Uses `snmpget` via ProcessStart. Adjust OIDs and interface indices to your environment.
- Cache results with `IMemoryCache` to reduce load.

#### Theming from GUI
- `SettingsController` persists `ThemeConfig` to DB. Frontend reads and applies.
- Update `styles.css` variables by injecting theme on load (see TODO below if you need to extend).

#### Testing
- Add unit tests under `tests/` (future). Mock `Db` and services.

