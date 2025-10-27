namespace SiparisApi.Services
{
    public class BcryptPasswordHasher : IPasswordHasher
    {
        public string Hash(string raw) => BCrypt.Net.BCrypt.HashPassword(raw, workFactor: 10);
        public bool Verify(string hash, string raw) => BCrypt.Net.BCrypt.Verify(raw, hash);
    }
}
