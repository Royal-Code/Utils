using System.Security.Cryptography;
using System.Text;

namespace RoyalCode.Extensions.SourceGenerator;

/// <summary>
/// Creates deterministic, collision-resistant source hint names with a bounded, readable prefix.
/// </summary>
/// <remarks>
/// <para>
///     The returned name has the format <c>{readable-name}.{hash}.g.cs</c>. The readable portion is
///     sanitized and limited to 32 characters. The hash contains the first 40 bits of the SHA-256 digest of
///     the complete identity, encoded as exactly eight file-name-safe Base32 characters.
/// </para>
/// <para>
///     Callers are responsible for building an identity that contains every component required for
///     uniqueness, such as namespace, containing types, metadata name, generated type and artifact category.
/// </para>
/// </remarks>
public static class GeneratedHintName
{
    private const int ReadableNameMaxLength = 32;
    private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    /// <summary>
    /// Creates a deterministic source hint name from a complete identity and a human-readable name.
    /// </summary>
    /// <param name="identity">
    ///     The complete, stable identity of the generated artifact. It is used only to compute the hash.
    /// </param>
    /// <param name="readableName">
    ///     The readable portion placed at the beginning of the file name. Non-alphanumeric characters other
    ///     than <c>_</c> and <c>-</c> are replaced with <c>_</c>, and the result is limited to 32 characters.
    /// </param>
    /// <returns>A deterministic hint name ending in <c>.g.cs</c>.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="identity"/> or <paramref name="readableName"/> is null, empty or
    ///     whitespace.
    /// </exception>
    public static string Create(string identity, string readableName)
    {
        if (string.IsNullOrWhiteSpace(identity))
            throw new ArgumentException("A hint name identity is required.", nameof(identity));
        if (string.IsNullOrWhiteSpace(readableName))
            throw new ArgumentException("A readable hint name is required.", nameof(readableName));

        var safeName = Sanitize(readableName);
        if (safeName.Length > ReadableNameMaxLength)
            safeName = safeName.Substring(0, ReadableNameMaxLength);

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(identity));

        return $"{safeName}.{EncodeBase32(hashBytes)}.g.cs";
    }

    private static string EncodeBase32(byte[] hash)
    {
        // Five bytes contain exactly the 40 bits represented by eight Base32 symbols, without padding.
        var encoded = new char[8];
        encoded[0] = Base32Alphabet[hash[0] >> 3];
        encoded[1] = Base32Alphabet[((hash[0] & 0x07) << 2) | (hash[1] >> 6)];
        encoded[2] = Base32Alphabet[(hash[1] >> 1) & 0x1F];
        encoded[3] = Base32Alphabet[((hash[1] & 0x01) << 4) | (hash[2] >> 4)];
        encoded[4] = Base32Alphabet[((hash[2] & 0x0F) << 1) | (hash[3] >> 7)];
        encoded[5] = Base32Alphabet[(hash[3] >> 2) & 0x1F];
        encoded[6] = Base32Alphabet[((hash[3] & 0x03) << 3) | (hash[4] >> 5)];
        encoded[7] = Base32Alphabet[hash[4] & 0x1F];
        return new string(encoded);
    }

    private static string Sanitize(string value)
    {
        var result = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            result.Append(char.IsLetterOrDigit(character) || character is '_' or '-'
                ? character
                : '_');
        }

        return result.ToString();
    }
}
