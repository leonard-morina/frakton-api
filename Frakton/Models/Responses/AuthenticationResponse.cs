namespace Frakton.Models.Responses
{
    public class AuthenticationResponse
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }


        public AuthenticationResponse(string id, string email, string token, string refreshToken)
        {
            Id = id;
            Email = email;
            Token = token;
            RefreshToken = refreshToken;
        }
    }
}