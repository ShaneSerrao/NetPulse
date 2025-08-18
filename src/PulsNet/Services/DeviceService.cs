using System.Net;
using System.Text.RegularExpressions;
using PulsNet.Data;

namespace PulsNet.Services
{
    public sealed class DeviceService
    {
        private readonly Db _db;

        public DeviceService(Db db)
        {
            _db = db;
        }

        public static bool IsValidIp(string ip)
        {
            return IPAddress.TryParse(ip, out _);
        }

        public static bool IsValidCommunity(string community)
        {
            return Regex.IsMatch(community, "^[A-Za-z0-9_.-]{1,64}$");
        }

        public async Task<List<DeviceRecord>> GetAllAsync(CancellationToken ct)
        {
            const string sql = "SELECT id, client_name, circuit_number, ip_address, snmp_community, max_link_mbps, per_client_interval_seconds FROM devices ORDER BY client_name";
            return await _db.QueryAsync(sql, r => new DeviceRecord
            {
                Id = r.GetInt32(0),
                ClientName = r.GetString(1),
                CircuitNumber = r.GetString(2),
                IpAddress = r.GetString(3),
                SnmpCommunity = r.GetString(4),
                MaxLinkMbps = r.GetInt32(5),
                PerClientIntervalSeconds = r.IsDBNull(6) ? null : r.GetInt32(6)
            }, null, ct);
        }

        public async Task<DeviceRecord?> GetByIdAsync(int id, CancellationToken ct)
        {
            const string sql = "SELECT id, client_name, circuit_number, ip_address, snmp_community, max_link_mbps, per_client_interval_seconds FROM devices WHERE id=@id";
            return await _db.QuerySingleAsync(sql, r => new DeviceRecord
            {
                Id = r.GetInt32(0),
                ClientName = r.GetString(1),
                CircuitNumber = r.GetString(2),
                IpAddress = r.GetString(3),
                SnmpCommunity = r.GetString(4),
                MaxLinkMbps = r.GetInt32(5),
                PerClientIntervalSeconds = r.IsDBNull(6) ? null : r.GetInt32(6)
            }, new { id }, ct);
        }

        public async Task<int> CreateAsync(DeviceRecord device, CancellationToken ct)
        {
            if (!IsValidIp(device.IpAddress)) throw new ArgumentException("Invalid IP");
            if (!IsValidCommunity(device.SnmpCommunity)) throw new ArgumentException("Invalid community");
            const string sql = @"INSERT INTO devices (client_name, circuit_number, ip_address, snmp_community, max_link_mbps, per_client_interval_seconds)
                                VALUES (@ClientName, @CircuitNumber, @IpAddress, @SnmpCommunity, @MaxLinkMbps, @PerClientIntervalSeconds) RETURNING id";
            var id = await _db.QuerySingleAsync(sql, r => r.GetInt32(0), device, ct);
            return id ?? 0;
        }

        public async Task UpdateAsync(DeviceRecord device, CancellationToken ct)
        {
            if (!IsValidIp(device.IpAddress)) throw new ArgumentException("Invalid IP");
            if (!IsValidCommunity(device.SnmpCommunity)) throw new ArgumentException("Invalid community");
            const string sql = @"UPDATE devices SET client_name=@ClientName, circuit_number=@CircuitNumber, ip_address=@IpAddress, snmp_community=@SnmpCommunity, max_link_mbps=@MaxLinkMbps, per_client_interval_seconds=@PerClientIntervalSeconds WHERE id=@Id";
            await _db.ExecuteAsync(sql, device, ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct)
        {
            await _db.ExecuteAsync("DELETE FROM devices WHERE id=@id", new { id }, ct);
        }
    }

    public sealed class DeviceRecord
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string CircuitNumber { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string SnmpCommunity { get; set; } = "public";
        public int MaxLinkMbps { get; set; }
        public int? PerClientIntervalSeconds { get; set; }
    }
}

