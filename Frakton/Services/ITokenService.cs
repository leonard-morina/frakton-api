namespace Frakton.Services
{
    public interface ITokenService
    {
        string GenerateToken(string userId, string secretKey);
    }
}