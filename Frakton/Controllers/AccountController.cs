using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frakton.Attributes;
using Frakton.Models;
using Frakton.Models.Responses;
using Frakton.Services;
using Frakton.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

namespace Frakton.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public AccountController(SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager, IConfiguration configuration,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel login)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(login.Email, login.Password, false, false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(login.Email);
                    var secretKey = _configuration["TokenConfig:Secret"];

                    //var tokenService = new JwtTokenService();
                    var token = _tokenService.GenerateToken(user.UserName, secretKey);
                    _tokenService.SaveRefreshToken(new UserRefreshToken
                        {RefreshToken = token.RefreshToken, Email = user.Email});
                    return Ok(new AuthenticationResponse(user.Id, user.UserName, token.Token, token.RefreshToken));
                }
            }
            else
            {
                var message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(message);
            }

            return NotFound();
        }

        [Route("check-authorization")]
        [HttpPost]
        [JwtAuthorize]
        public IActionResult CheckTokenAuthorization()
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var secret = _configuration["TokenConfig:Secret"];
                var key = Encoding.ASCII.GetBytes(secret);
                var token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,

                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken) validatedToken;
                var user = jwtToken.Claims.First(x => x.Type == "id").Value;
                return Ok(user);
            }
            catch
            {
                //
            }

            return BadRequest("Not Authorized");
        }

        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel register)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    FirstName = register.FirstName, LastName = register.LastName,
                    UserName = register.Email, Email = register.Email
                };
                var result = await _userManager.CreateAsync(user, register.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, false);
                    return Ok("Signed up successfully");
                }

                var errorMessage = string.Join(" | ", result.Errors
                    .Select(e => e.Description));
                return BadRequest(errorMessage);
            }

            var message = string.Join(" | ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            return BadRequest(message);
        }

        [Route("refreshtoken")]
        [HttpPost]
        public IActionResult RefreshToken(JwtToken jwtToken)
        {
            if (jwtToken == null) return BadRequest();
            var handler = new JwtSecurityTokenHandler();
            SecurityToken validatedToken;

            var secretKey = _configuration["TokenConfig:Secret"];
            var secretInBytes = Encoding.UTF8.GetBytes(secretKey);

            var principal = handler.ValidateToken(jwtToken.Token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretInBytes),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false
            }, out validatedToken);


            var validatedSecurityToken = (JwtSecurityToken)validatedToken;
            var username = validatedSecurityToken.Claims.First(x => x.Type == "id").Value;

            if (_tokenService.IsRefreshTokenValid(username, jwtToken.RefreshToken))
            {
                var newToken = _tokenService.GenerateToken(username, secretKey);
                _tokenService.SaveRefreshToken(new UserRefreshToken
                {
                    Email = username,
                    RefreshToken = newToken.RefreshToken
                });

                return Ok(newToken);
            }

            return BadRequest();
        }
    }
}