using System.Security.Cryptography;

namespace RaceDay.Api.Services;

public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public class PasswordService : IPasswordService
{
    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty.", nameof(password));

        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            120000,
            HashAlgorithmName.SHA256,
            32);

        return $"PBKDF2$120000${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        var parts = hash.Split('$');

        if (parts.Length != 4 || parts[0] != "PBKDF2")
            return false;

        try
        {
            var iterations = int.Parse(parts[1]);
            var salt = Convert.FromBase64String(parts[2]);
            var expectedHash = Convert.FromBase64String(parts[3]);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var actualHash = pbkdf2.GetBytes(32);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch
        {
            return false;
        }
    }
}
