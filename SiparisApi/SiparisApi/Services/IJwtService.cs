namespace SiparisApi.Services
{
    public interface IJwtService
    {
        string CreateToken(int userId, string email);
    }
}
