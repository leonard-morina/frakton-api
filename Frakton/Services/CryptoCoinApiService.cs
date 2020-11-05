using System.Collections.Generic;
using System.Threading.Tasks;
using Frakton.Models;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace Frakton.Services
{
    public class CryptoCoinApiService
    {
        private readonly IConfiguration _configuration;

        public CryptoCoinApiService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IRestResponse> GetCryptoCoinResponseFromApiAsync()
        {
            var url = _configuration["ApiConfig:Url"];
            var client = new RestClient(url);

            var request = new RestRequest("assets", DataFormat.Json);
            var response = await client.ExecuteAsync<List<CryptoCoin>>(request);
            return response;
        }
    }
}
