using System.Security.Cryptography;
namespace RaceDay.Api.Services;
public interface IPasswordService { string Hash(string password); bool Verify(string password,string hash); }
public class PasswordService : IPasswordService
{
    public string Hash(string password) { var salt=RandomNumberGenerator.GetBytes(16); var hash=Rfc2898DeriveBytes.Pbkdf2(password,salt,120000,HashAlgorithmName.SHA256,32); return $"PBKDF2$120000$SHA256${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}"; }
    public bool Verify(string password,string encoded) { try { var parts=encoded.Split('$'); if(parts.Length!=5||parts[0]!="PBKDF2") return false; var iterations=int.Parse(parts[1]); var salt=Convert.FromBase64String(parts[3]); var expected=Convert.FromBase64String(parts[4]); var actual=Rfc2898DeriveBytes.Pbkdf2(password,salt,iterations,HashAlgorithmName.SHA256,expected.Length); return CryptographicOperations.FixedTimeEquals(actual,expected); } catch { return false; } }
}
