namespace SiparisApi.Services
{
    public interface IPasswordHasher
    {
        string Hash(string raw);
        bool Verify(string hash, string raw);
    }
}
