using System.Security.Cryptography;
using System.Text;

namespace Zyga.HikIsapiClient.ExtensionMethods;

/// <summary>
/// String helpers used to implement ISAPI's digest-style challenge/response login.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Computes the password hash ISAPI expects in response to a login challenge.
    /// </summary>
    /// <param name="password">The account password.</param>
    /// <param name="challenge">The challenge string returned by the device.</param>
    /// <param name="userName">The account user name.</param>
    /// <param name="salt">The salt returned by the device.</param>
    /// <param name="iterations">The number of hash iterations the device requires.</param>
    /// <param name="isIrreversible">
    /// Whether the device uses the irreversible (salted, user-name-bound) hashing scheme.
    /// </param>
    /// <returns>The hash to submit as the login password.</returns>
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

    /// <summary>
    /// Computes the lowercase hexadecimal SHA-256 hash of a string.
    /// </summary>
    /// <param name="value">The string to hash.</param>
    /// <returns>The hash, as a lowercase hexadecimal string.</returns>
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
