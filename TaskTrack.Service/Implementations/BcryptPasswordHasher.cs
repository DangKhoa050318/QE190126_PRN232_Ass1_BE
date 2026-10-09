using TaskTrack.Service.Interfaces;

namespace TaskTrack.Service.Implementations;

public class BcryptPasswordHasher : IPasswordHasher
{
    // Cost 11 (~100-200 ms per hash): slow enough to resist brute force, fast enough for Render's free CPU.
    private const int WorkFactor = 11;

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false; // malformed hash in the database
        }
    }
}
