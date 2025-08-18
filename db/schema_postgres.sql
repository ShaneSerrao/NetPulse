-- PulsNet PostgreSQL schema

CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    username TEXT UNIQUE NOT NULL,
    role TEXT NOT NULL CHECK (role IN ('Admin','User')),
    password_hash TEXT NOT NULL,
    password_salt TEXT NOT NULL,
    two_factor_enabled BOOLEAN DEFAULT FALSE,
    two_factor_secret TEXT NULL,
    email TEXT NULL,
    failed_attempts INT DEFAULT 0,
    lockout_until_utc TIMESTAMPTZ NULL,
    created_utc TIMESTAMPTZ DEFAULT now()
);

CREATE TABLE IF NOT EXISTS devices (
    id SERIAL PRIMARY KEY,
    client_name TEXT NOT NULL,
    circuit_number TEXT NOT NULL,
    ip_address TEXT NOT NULL,
    snmp_community TEXT NOT NULL,
    max_link_mbps INT NOT NULL,
    per_client_interval_seconds INT NULL,
    created_utc TIMESTAMPTZ DEFAULT now()
);

CREATE TABLE IF NOT EXISTS settings (
    key TEXT PRIMARY KEY,
    value_json TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS traffic_stats (
    id BIGSERIAL PRIMARY KEY,
    device_id INT NOT NULL REFERENCES devices(id) ON DELETE CASCADE,
    ts_utc TIMESTAMPTZ NOT NULL,
    down_mbps DOUBLE PRECISION NOT NULL,
    up_mbps DOUBLE PRECISION NOT NULL,
    latency_ms DOUBLE PRECISION NOT NULL,
    online BOOLEAN NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_traffic_stats_device_time ON traffic_stats(device_id, ts_utc DESC);

-- Seed admin user (password: admin123 - change in production)
-- Replace with a secure process during deployment
INSERT INTO users (username, role, password_hash, password_salt)
VALUES ('admin', 'Admin', 'REPLACE_HASH', 'REPLACE_SALT')
ON CONFLICT (username) DO NOTHING;

