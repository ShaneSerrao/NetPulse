using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using PulsNet.Data;

namespace PulsNet.Services
{
    public sealed class AuthService
    {
        private readonly Db _db;
        private readonly IHttpContextAccessor _http;
        private readonly ConfigService _configService;

        public AuthService(Db db, IHttpContextAccessor http, ConfigService configService)
        {
            _db = db;
            _http = http;
            _configService = configService;
        }

        public async Task<(bool ok, string? reason, UserRecord? user)> AuthenticateAsync(string username, string password, string? totpCode, CancellationToken ct)
        {
            var user = await GetUserByUsernameAsync(username, ct);
            if (user == null) return (false, "Invalid credentials", null);

            if (user.LockoutUntilUtc.HasValue && user.LockoutUntilUtc.Value > DateTime.UtcNow)
            {
                return (false, "Account locked. Try later.", null);
            }

            var valid = VerifyPassword(password, user.PasswordSalt, user.PasswordHash);
            if (!valid)
            {
                await IncrementFailedAsync(user.Id, ct);
                return (false, "Invalid credentials", null);
            }

            var global2fa = _configService.Config.Security.Global2FAEnabled;
            if (global2fa || user.TwoFactorEnabled)
            {
                if (string.IsNullOrWhiteSpace(totpCode) || !VerifyTotp(user.TwoFactorSecret ?? string.Empty, totpCode))
                {
                    return (false, "2FA required or invalid", null);
                }
            }

            await ResetFailedAsync(user.Id, ct);
            return (true, null, user);
        }

        public async Task<UserRecord?> GetUserByUsernameAsync(string username, CancellationToken ct)
        {
            return await _db.QuerySingleAsync("SELECT id, username, role, password_hash, password_salt, two_factor_enabled, two_factor_secret, email, failed_attempts, lockout_until_utc FROM users WHERE username=@username",
                r => new UserRecord
                {
                    Id = r.GetInt32(0),
                    Username = r.GetString(1),
                    Role = r.GetString(2),
                    PasswordHash = r.GetString(3),
                    PasswordSalt = r.GetString(4),
                    TwoFactorEnabled = r.IsDBNull(5) ? false : r.GetBoolean(5),
                    TwoFactorSecret = r.IsDBNull(6) ? null : r.GetString(6),
                    Email = r.IsDBNull(7) ? null : r.GetString(7),
                    FailedAttempts = r.IsDBNull(8) ? 0 : r.GetInt32(8),
                    LockoutUntilUtc = r.IsDBNull(9) ? null : r.GetDateTime(9)
                }, new { username }, ct);
        }

        public async Task CreateUserAsync(string username, string password, string role, string? email, CancellationToken ct)
        {
            var (salt, hash) = HashPassword(password);
            await _db.ExecuteAsync("INSERT INTO users (username, role, password_hash, password_salt, email) VALUES (@username,@role,@hash,@salt,@email)", new { username, role, hash, salt, email }, ct);
        }

        public async Task Enable2FAAsync(int userId, string secret, CancellationToken ct)
        {
            await _db.ExecuteAsync("UPDATE users SET two_factor_enabled=true, two_factor_secret=@secret WHERE id=@userId", new { userId, secret }, ct);
        }

        public static (string salt, string hash) HashPassword(string password)
        {
            var saltBytes = RandomNumberGenerator.GetBytes(16);
            var hashBytes = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, 100_000, HashAlgorithmName.SHA256, 32);
            return (Convert.ToBase64String(saltBytes), Convert.ToBase64String(hashBytes));
        }

        public static bool VerifyPassword(string password, string saltBase64, string hashBase64)
        {
            var salt = Convert.FromBase64String(saltBase64);
            var hash = Convert.FromBase64String(hashBase64);
            var newHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(newHash, hash);
        }

        public static string GenerateTotpSecret()
        {
            var secret = RandomNumberGenerator.GetBytes(20);
            return Base32Encode(secret);
        }

        public static bool VerifyTotp(string base32Secret, string code, int timeStepSeconds = 30, int allowedDriftSteps = 1)
        {
            if (string.IsNullOrWhiteSpace(base32Secret)) return false;
            if (string.IsNullOrWhiteSpace(code) || code.Length is < 6 or > 8) return false;
            if (!int.TryParse(code, out _)) return false;

            var key = Base32Decode(base32Secret);
            var unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var timestep = unix / timeStepSeconds;

            for (var i = -allowedDriftSteps; i <= allowedDriftSteps; i++)
            {
                var counter = BitConverter.GetBytes((long)(timestep + i));
                if (BitConverter.IsLittleEndian) Array.Reverse(counter);
                using var hmac = new HMACSHA1(key);
                var hs = hmac.ComputeHash(counter);
                var offset = hs[hs.Length - 1] & 0x0F;
                var binary = ((hs[offset] & 0x7f) << 24) | (hs[offset + 1] << 16) | (hs[offset + 2] << 8) | hs[offset + 3];
                var otp = binary % 1_000_000;
                if (otp.ToString("D6") == code) return true;
            }
            return false;
        }

        private static string Base32Encode(byte[] data)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var output = new StringBuilder();
            int bitBuffer = 0, bitBufferLength = 0;
            foreach (var b in data)
            {
                bitBuffer = (bitBuffer << 8) | b;
                bitBufferLength += 8;
                while (bitBufferLength >= 5)
                {
                    var index = (bitBuffer >> (bitBufferLength - 5)) & 31;
                    bitBufferLength -= 5;
                    output.Append(alphabet[index]);
                }
            }
            if (bitBufferLength > 0)
            {
                var index = (bitBuffer << (5 - bitBufferLength)) & 31;
                output.Append(alphabet[index]);
            }
            return output.ToString();
        }

        private static byte[] Base32Decode(string input)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var map = alphabet.Select((c, i) => (c, i)).ToDictionary(t => t.c, t => t.i);
            int bitBuffer = 0, bitBufferLength = 0;
            var bytes = new List<byte>();
            foreach (var ch in input.ToUpperInvariant().Where(c => c != '='))
            {
                if (!map.TryGetValue(ch, out var val)) continue;
                bitBuffer = (bitBuffer << 5) | val;
                bitBufferLength += 5;
                if (bitBufferLength >= 8)
                {
                    bytes.Add((byte)((bitBuffer >> (bitBufferLength - 8)) & 0xFF));
                    bitBufferLength -= 8;
                }
            }
            return bytes.ToArray();
        }

        private async Task IncrementFailedAsync(int userId, CancellationToken ct)
        {
            await _db.ExecuteAsync("UPDATE users SET failed_attempts = COALESCE(failed_attempts,0)+1 WHERE id=@userId", new { userId }, ct);
            var user = await _db.QuerySingleAsync("SELECT failed_attempts FROM users WHERE id=@userId", r => r.GetInt32(0), new { userId }, ct);
            var max = _configService.Config.Security.BruteForceProtection.MaxAttempts;
            if ((user ?? 0) >= max)
            {
                var mins = _configService.Config.Security.BruteForceProtection.LockoutMinutes;
                await _db.ExecuteAsync("UPDATE users SET lockout_until_utc = @until WHERE id=@userId", new { until = DateTime.UtcNow.AddMinutes(mins), userId }, ct);
            }
        }

        private async Task ResetFailedAsync(int userId, CancellationToken ct)
        {
            await _db.ExecuteAsync("UPDATE users SET failed_attempts = 0, lockout_until_utc=NULL WHERE id=@userId", new { userId }, ct);
        }
    }

    public sealed class UserRecord
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public string PasswordHash { get; set; } = string.Empty;
        public string PasswordSalt { get; set; } = string.Empty;
        public bool TwoFactorEnabled { get; set; }
        public string? TwoFactorSecret { get; set; }
        public string? Email { get; set; }
        public int FailedAttempts { get; set; }
        public DateTime? LockoutUntilUtc { get; set; }
    }
}

