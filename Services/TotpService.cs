using System.Security.Cryptography;
using System.Text;

namespace PulsNet.Services {
  public static class TotpService {
    private const int StepSeconds = 30;
    private const int CodeDigits = 6;
    private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    public static string GenerateSecret(int bytes = 20){
      var buf = RandomNumberGenerator.GetBytes(bytes);
      return Base32Encode(buf);
    }

    public static bool Validate(string base32Secret, string code, int window = 1){
      if (string.IsNullOrWhiteSpace(code) || code.Length < 6) return false;
      if (!int.TryParse(code, out _)) return false;
      try{
        var key = Base32Decode(base32Secret);
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var step = now / StepSeconds;
        for(int w=-window; w<=window; w++){
          var cur = ComputeCode(key, (ulong)(step + w));
          if (TimingEqual(cur, code)) return true;
        }
        return false;
      }catch{ return false; }
    }

    public static string BuildOtpAuthUri(string issuer, string account, string base32Secret){
      var label = Uri.EscapeDataString($"{issuer}:{account}");
      var iss = Uri.EscapeDataString(issuer);
      return $"otpauth://totp/{label}?secret={base32Secret}&issuer={iss}&algorithm=SHA1&digits={CodeDigits}&period={StepSeconds}";
    }

    private static string ComputeCode(byte[] key, ulong counter){
      var cnt = BitConverter.GetBytes(counter);
      if (BitConverter.IsLittleEndian) Array.Reverse(cnt);
      using var hmac = new HMACSHA1(key);
      var hash = hmac.ComputeHash(cnt);
      int offset = hash[^1] & 0x0F;
      int binary = ((hash[offset] & 0x7F) << 24) | (hash[offset + 1] << 16) | (hash[offset + 2] << 8) | (hash[offset + 3]);
      int otp = binary % (int)Math.Pow(10, CodeDigits);
      return otp.ToString(new string('0', CodeDigits));
    }

    private static bool TimingEqual(string a, string b){
      if (a.Length != b.Length) return false;
      int r = 0; for (int i=0;i<a.Length;i++){ r |= a[i] ^ b[i]; }
      return r == 0;
    }

    private static string Base32Encode(byte[] data){
      var output = new StringBuilder((data.Length + 7) * 8 / 5);
      int buffer = data[0];
      int next = 1;
      int bitsLeft = 8;
      while (bitsLeft > 0 || next < data.Length){
        if (bitsLeft < 5){
          if (next < data.Length){
            buffer <<= 8;
            buffer |= data[next++] & 0xff;
            bitsLeft += 8;
          } else {
            int pad = 5 - bitsLeft;
            buffer <<= pad;
            bitsLeft += pad;
          }
        }
        int index = 0x1f & (buffer >> (bitsLeft - 5));
        bitsLeft -= 5;
        output.Append(Base32Alphabet[index]);
      }
      return output.ToString();
    }

    private static byte[] Base32Decode(string input){
      var clean = input.Trim().ToUpperInvariant().Replace(" ", "");
      int buffer = 0, bitsLeft = 0;
      var result = new List<byte>(clean.Length * 5 / 8);
      foreach (var c in clean){
        int val = Base32Alphabet.IndexOf(c);
        if (val < 0) continue;
        buffer <<= 5;
        buffer |= val & 31;
        bitsLeft += 5;
        if (bitsLeft >= 8){
          result.Add((byte)((buffer >> (bitsLeft - 8)) & 0xff));
          bitsLeft -= 8;
        }
      }
      return result.ToArray();
    }
  }
}

