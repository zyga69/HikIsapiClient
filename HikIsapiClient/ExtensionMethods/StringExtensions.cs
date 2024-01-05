using System.Security.Cryptography;
using System.Text;

namespace HikIsapi.Client.ExtensionMethods;

public static class StringExtensions
{
    public static string ToPasswordHash(this string password, string challenge, string userName, string salt, int iterations, bool isIrreversible)
    {
        string? challengeHash;
        if (isIrreversible)
        {
            var loginHash = $"{userName}{salt}{password}".ToSha256Hash();
            challengeHash = $"{loginHash}{challenge}".ToSha256Hash();

            for (var i = 2; i < iterations; i++)
                challengeHash = challengeHash.ToSha256Hash();
        }
        else
        {
            challengeHash = $"{password}{challenge}";

            for (var i = 1; i < iterations; i++)
                challengeHash = challengeHash.ToSha256Hash();
        }

        return challengeHash;
    }

    public static string ToSha256Hash(this string value)
    {
        StringBuilder Sb = new StringBuilder();
        Encoding enc = Encoding.UTF8;
        byte[] result = SHA256.HashData(enc.GetBytes(value));

        foreach (byte b in result)
            Sb.Append(b.ToString("x2"));

        return Sb.ToString();
    }
}
