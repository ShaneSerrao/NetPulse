ALTER TABLE users DROP CONSTRAINT IF EXISTS users_role_check;
ALTER TABLE users ADD CONSTRAINT users_role_check CHECK (role IN ('SuperAdmin','Admin','Operator','Viewer'));

CREATE TABLE IF NOT EXISTS tenants (id SERIAL PRIMARY KEY, name TEXT NOT NULL UNIQUE, created_utc TIMESTAMPTZ DEFAULT now());
CREATE TABLE IF NOT EXISTS device_tenants (device_id INT NOT NULL REFERENCES devices(id) ON DELETE CASCADE, tenant_id INT NOT NULL REFERENCES tenants(id) ON DELETE CASCADE, PRIMARY KEY (device_id, tenant_id));

CREATE TABLE IF NOT EXISTS "ConfigTemplates" ("Id" SERIAL PRIMARY KEY, "Name" TEXT NOT NULL, "Content" TEXT NOT NULL, "Version" INT NOT NULL DEFAULT 1, "TenantId" INT NULL REFERENCES tenants(id) ON DELETE SET NULL, "CreatedUtc" TIMESTAMPTZ DEFAULT now());
CREATE INDEX IF NOT EXISTS ix_configtemplates_tenant ON "ConfigTemplates" ("TenantId");

CREATE TABLE IF NOT EXISTS "Scripts" ("Id" SERIAL PRIMARY KEY, "Name" TEXT NOT NULL, "ScriptText" TEXT NOT NULL, "Version" INT NOT NULL DEFAULT 1, "TenantId" INT NULL REFERENCES tenants(id) ON DELETE SET NULL, "CreatedUtc" TIMESTAMPTZ DEFAULT now());
CREATE INDEX IF NOT EXISTS ix_scripts_tenant ON "Scripts" ("TenantId");

CREATE TABLE IF NOT EXISTS "ConfigHistory" ("Id" BIGSERIAL PRIMARY KEY, "DeviceId" INT NOT NULL REFERENCES devices(id) ON DELETE CASCADE, "ActionType" TEXT NOT NULL, "UserId" INT NULL REFERENCES users(id) ON DELETE SET NULL, "Timestamp" TIMESTAMPTZ NOT NULL DEFAULT now(), "OldConfig" TEXT NULL, "NewConfig" TEXT NULL, "Status" TEXT NOT NULL, "ActionId" BIGINT NULL);
CREATE INDEX IF NOT EXISTS ix_confighistory_device_time ON "ConfigHistory" ("DeviceId","Timestamp" DESC);

CREATE TABLE IF NOT EXISTS management_actions (id BIGSERIAL PRIMARY KEY, action_type TEXT NOT NULL, initiated_by_user_id INT NULL REFERENCES users(id) ON DELETE SET NULL, payload_json TEXT NOT NULL, status TEXT NOT NULL DEFAULT 'Queued', progress_percent INT NOT NULL DEFAULT 0, error TEXT NULL, created_utc TIMESTAMPTZ NOT NULL DEFAULT now(), started_utc TIMESTAMPTZ NULL, completed_utc TIMESTAMPTZ NULL);
CREATE TABLE IF NOT EXISTS management_action_devices (action_id BIGINT NOT NULL REFERENCES management_actions(id) ON DELETE CASCADE, device_id INT NOT NULL REFERENCES devices(id) ON DELETE CASCADE, PRIMARY KEY (action_id, device_id));

CREATE TABLE IF NOT EXISTS mib_categories (id SERIAL PRIMARY KEY, name TEXT NOT NULL UNIQUE);
CREATE TABLE IF NOT EXISTS mib_catalog (id SERIAL PRIMARY KEY, oid TEXT NOT NULL UNIQUE, name TEXT NOT NULL, category_id INT NULL REFERENCES mib_categories(id) ON DELETE SET NULL, description TEXT NULL);
CREATE INDEX IF NOT EXISTS ix_mib_catalog_category ON mib_catalog(category_id);
CREATE TABLE IF NOT EXISTS mib_device_selections (device_id INT NOT NULL REFERENCES devices(id) ON DELETE CASCADE, mib_id INT NOT NULL REFERENCES mib_catalog(id) ON DELETE CASCADE, PRIMARY KEY (device_id, mib_id));
CREATE TABLE IF NOT EXISTS mib_walk_logs (id BIGSERIAL PRIMARY KEY, device_id INT NOT NULL REFERENCES devices(id) ON DELETE CASCADE, base_oid TEXT NOT NULL, output TEXT NOT NULL, created_utc TIMESTAMPTZ NOT NULL DEFAULT now());

CREATE TABLE IF NOT EXISTS sites (id SERIAL PRIMARY KEY, name TEXT NOT NULL, address TEXT NULL, latitude DOUBLE PRECISION NULL, longitude DOUBLE PRECISION NULL, created_utc TIMESTAMPTZ NOT NULL DEFAULT now());
CREATE TABLE IF NOT EXISTS device_sites (device_id INT NOT NULL REFERENCES devices(id) ON DELETE CASCADE, site_id INT NOT NULL REFERENCES sites(id) ON DELETE CASCADE, PRIMARY KEY (device_id, site_id));

ALTER TABLE devices ADD COLUMN IF NOT EXISTS interface_index INT NULL;
ALTER TABLE devices ADD COLUMN IF NOT EXISTS cap_down_enabled BOOLEAN NOT NULL DEFAULT FALSE;
ALTER TABLE devices ADD COLUMN IF NOT EXISTS cap_up_enabled BOOLEAN NOT NULL DEFAULT FALSE;
ALTER TABLE devices ADD COLUMN IF NOT EXISTS cap_down_mbps INT NULL;
ALTER TABLE devices ADD COLUMN IF NOT EXISTS cap_up_mbps INT NULL;

INSERT INTO mib_categories(name) VALUES ('System') ON CONFLICT (name) DO NOTHING;
INSERT INTO mib_categories(name) VALUES ('Interfaces') ON CONFLICT (name) DO NOTHING;
INSERT INTO mib_categories(name) VALUES ('CPU') ON CONFLICT (name) DO NOTHING;
INSERT INTO mib_categories(name) VALUES ('Memory') ON CONFLICT (name) DO NOTHING;
INSERT INTO mib_catalog(oid, name, category_id, description)
SELECT '1.3.6.1.2.1.1.3.0','sysUpTime', c.id, 'System Uptime'
FROM mib_categories c WHERE c.name='System'
ON CONFLICT (oid) DO NOTHING;
