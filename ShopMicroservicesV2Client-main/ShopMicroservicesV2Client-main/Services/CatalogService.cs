using AspnetRunBasics.Extensions;
using AspnetRunBasics.Models;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspnetRunBasics.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly HttpClient _client;
        private readonly IHttpClientFactory _clientFactory;
        IConfiguration _configuration;
        public CatalogService(HttpClient client, ILogger<CatalogService> logger, IConfiguration configuration,IHttpClientFactory clientFactory)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _configuration = configuration ?? throw new ArgumentNullException();
           _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        }

        public async Task<IEnumerable<CatalogModel>> GetCatalog()
        {
            //way 1
            var httpClient = _clientFactory.CreateClient("ShopAPIClient");

            var request = new HttpRequestMessage(
                HttpMethod.Get, //"http://localhost:6002/api/Discount/IPhone%20X");
                                     _configuration["ApiSettings:GatewayAddress"] + "/Catalog");

            var response = await httpClient.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

           

            return await response.ReadContentAs<List<CatalogModel>>();
      
            /*
            //demo way 2
            var apiClientCredentials = new ClientCredentialsTokenRequest
            {

               Address = "https://localhost:5005/connect/token",

               ClientId = "shopClient",
               ClientSecret = "secret",

                // This is the scope our Protected API requires. 
                Scope = "shopAPI"
            };
            //// creates a new HttpClient to talk to our IdentityServer (localhost:5005)
            //// just checks if we can reach the Discovery document. Not 100% needed but..
            var disco = await _client.GetDiscoveryDocumentAsync("https://localhost:5005");
            if (disco.IsError)
            {
                return null; // throw 500 error
            }
            //// 2. Authenticates and get an access token from Identity Server
            var tokenResponse = await _client.RequestClientCredentialsTokenAsync(apiClientCredentials);            
            if (tokenResponse.IsError)
            {
                return null;
            }

            //// Another HttpClient for talking now with our Protected API
            var apiClient = new HttpClient();

            //// 3. Set the access_token in the request Authorization: Bearer <token>
            apiClient.SetBearerToken(tokenResponse.AccessToken);
            // ==> if config this api with jwt Bearer

            //// 4. Send a request to our Protected API
            var response = await apiClient.GetAsync(_configuration["ApiSettings:GatewayAddress"]+"/Catalog");
            response.EnsureSuccessStatusCode();

            
            return await response.ReadContentAs<List<CatalogModel>>();
            */
        }

        public async Task<CatalogModel> GetCatalog(string id)
        {
            var response = await _client.GetAsync($"/Catalog/{id}");
            return await response.ReadContentAs<CatalogModel>();
        }

        public async Task<IEnumerable<CatalogModel>> GetCatalogByCategory(string category)
        {
            var response = await _client.GetAsync($"/Catalog/GetProductByCategory/{category}");
            return await response.ReadContentAs<List<CatalogModel>>();
        }

        public async Task<CatalogModel> CreateCatalog(CatalogModel model)
        {            
            var response = await _client.PostAsJson($"/Catalog", model);
            if (response.IsSuccessStatusCode)
                return await response.ReadContentAs<CatalogModel>();
            else
            {
                throw new Exception("Something went wrong when calling api.");
            }
        }        
    }
}
