using Frakton.Models;

namespace Frakton.Services
{
    public interface ITokenService
    {
        JwtToken GenerateToken(string userId, string secretKey);
        string GenerateRefreshToken(int size = 32);
        void SaveRefreshToken(UserRefreshToken userRefreshToken);
        bool IsRefreshTokenValid(string username, string refreshToken);
    }
}
