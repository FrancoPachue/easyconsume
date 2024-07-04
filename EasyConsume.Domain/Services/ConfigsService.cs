using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EasyConsume.Domain.Services
{
    public class ConfigsService
    {
        private readonly ILogger<ConfigsService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ConfigsService(IHttpClientFactory httpClientFactory, ILogger<ConfigsService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        public async Task<string> GetStringAsync(string client, string endpoint)
        {
            try
            {
                using (var httpClient = _httpClientFactory.CreateClient($"{client}"))
                {
                    var response = await httpClient.GetAsync($"/{endpoint}");
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync();

                    return content;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error obtaining configs: {ex.Message}", ex);
            }
        }
    }
}
