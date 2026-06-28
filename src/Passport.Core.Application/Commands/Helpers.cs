using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Passport.Core.Application.Commands;

internal static class Helpers
{
    public static string GenerateCode()
    {
        return RandomNumberGenerator.GetInt32(0, 1000000).ToString("D6", CultureInfo.InvariantCulture);
    }

    public static string HashCode(string code)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexStringLower(hash);
    }
}