using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Frakton.Attributes;
using Frakton.Data;
using Frakton.Models;
using Frakton.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;

namespace Frakton.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CryptoCoinController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly CryptoCoinApiService _cryptoCoinApiService;
        private readonly ApplicationDbContext _context;

        public CryptoCoinController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager, 
            IConfiguration configuration, CryptoCoinApiService cryptoCoinApiService)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _cryptoCoinApiService = cryptoCoinApiService;
        }
        [Route("list")]
        [JwtAuthorize]
        [HttpGet]
        public async Task<IActionResult> GetCryptoCoins()
        {
            var response = await _cryptoCoinApiService.GetCryptoCoinResponseFromApiAsync();
            if (response.StatusCode != HttpStatusCode.OK) return StatusCode((int) response.StatusCode);
            CryptoCoinResult result = JsonConvert.DeserializeObject<CryptoCoinResult>(response.Content);
            return Ok(result);
        }

        [Route("favorite/mark/{cryptoCoinId}")]
        [JwtAuthorize]
        [HttpPost]
        public async Task<IActionResult> MarkAsFavorite(string cryptoCoinId)
        {
            var response = await _cryptoCoinApiService.GetCryptoCoinResponseFromApiAsync();

            if (response.StatusCode != HttpStatusCode.OK) return StatusCode((int)response.StatusCode);
            var result = JsonConvert.DeserializeObject<CryptoCoinResult>(response.Content);
            if (result.Data.Count <= 0) return NoContent();
            var cryptoCoin = result.Data.FirstOrDefault(c => c.Id == cryptoCoinId);
            if (cryptoCoin == null) return NotFound();

            string email = (string) HttpContext.Items["User"];
            var user = await _userManager.FindByNameAsync(email);

            //no need to validate if the user exists or not since the user can't get to the method in the first place if he's not authenticated
            _context.FavoriteCryptoCoins.Add(new FavoriteCryptoCoin
                {CryptoCoinId = cryptoCoinId, UserId = user.Id, InsertedOn = DateTime.Now});
            bool inserted = _context.SaveChanges() > 0;
            if (!inserted) return NotFound();
            return Ok(cryptoCoin);
        }

        [Route("favorite/unmark/{cryptoCoinId}")]
        [JwtAuthorize]
        [HttpDelete]

        public async Task<IActionResult> UnMarkFromFavorite(string cryptoCoinId)
        {
            var email = (string)HttpContext.Items["User"];
            var user = await _userManager.FindByNameAsync(email);

            var favoriteCryptoCoin =
                _context.FavoriteCryptoCoins.FirstOrDefault(f => f.CryptoCoinId == cryptoCoinId && f.UserId == user.Id);
            if (favoriteCryptoCoin == null)
                return BadRequest("No favorite crypto coin was found with this specific cryptoCoinId");
            _context.FavoriteCryptoCoins.Remove(favoriteCryptoCoin);
            bool deleted = _context.SaveChanges() > 0;
            if (!deleted) return BadRequest();
            return Ok("The crypto coin was unmarked from favorite successfully");
        }

        [Route("favorites")]
        [JwtAuthorize]
        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            var email = (string) HttpContext.Items["User"];
            var user = await _userManager.FindByNameAsync(email);
            var favoriteCryptoCoinIds = _context.FavoriteCryptoCoins.Where(fc => fc.UserId == user.Id)
                .Select(e => e.CryptoCoinId).ToList();

            var response = await _cryptoCoinApiService.GetCryptoCoinResponseFromApiAsync();

            if (response.StatusCode != HttpStatusCode.OK) return StatusCode((int)response.StatusCode);
            CryptoCoinResult result = JsonConvert.DeserializeObject<CryptoCoinResult>(response.Content);
            var favoriteCryptoCoins =
                result.Data.Where(fcc => favoriteCryptoCoinIds.Contains(fcc.Id));
            return Ok(favoriteCryptoCoins);
        }
    }
}