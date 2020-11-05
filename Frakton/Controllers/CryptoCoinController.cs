﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Frakton.Attributes;
using Frakton.Data;
using Frakton.Models;
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
        private readonly ApplicationDbContext _context;
        public CryptoCoinController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;

        }
        [Route("list")]
        [JwtAuthorize]
        [HttpGet]
        public async Task<IActionResult> GetCryptoCoins()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var url = configuration["ApiConfig:Url"];
            var client = new RestClient(url);

            var request = new RestRequest("assets", DataFormat.Json);
            var response = await client.ExecuteAsync<List<CryptoCoin>>(request);
            if (response.StatusCode != HttpStatusCode.OK) return StatusCode((int) response.StatusCode);
            CryptoCoinResult result = JsonConvert.DeserializeObject<CryptoCoinResult>(response.Content);
            return Ok(result);
        }

        [Route("favorite/mark/{cryptoCoinId}")]
        [JwtAuthorize]
        [HttpPost]
        public async Task<IActionResult> MarkAsFavorite(string cryptoCoinId)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var url = configuration["ApiConfig:Url"];
            var client = new RestClient(url);


            var request = new RestRequest("assets", DataFormat.Json);
            var response = await client.ExecuteAsync<List<CryptoCoin>>(request);
            if (response.StatusCode != HttpStatusCode.OK) return StatusCode((int)response.StatusCode);
            CryptoCoinResult result = JsonConvert.DeserializeObject<CryptoCoinResult>(response.Content);
            if (result.Data.Count <= 0) return NoContent();
            var cryptoCoin = result.Data.FirstOrDefault(c => c.Id == cryptoCoinId);
            if (cryptoCoin == null) return NotFound();

            string email = (string)HttpContext.Items["User"];
            ApplicationUser user = await _userManager.FindByNameAsync(email);

            //no need to validate if the user exists or not since the user can't get to the method in the first place if he's not authenticated
            _context.FavoriteCryptoCoins.Add(new FavoriteCryptoCoin()
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
            string email = (string)HttpContext.Items["User"];
            ApplicationUser user = await _userManager.FindByNameAsync(email);

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
            string email = (string)HttpContext.Items["User"];
            ApplicationUser user = await _userManager.FindByNameAsync(email);

            var favoriteCryptoCoins = _context.FavoriteCryptoCoins.Where(fc => fc.UserId == user.Id).ToList();
            return Ok(favoriteCryptoCoins);
        }
    }
}