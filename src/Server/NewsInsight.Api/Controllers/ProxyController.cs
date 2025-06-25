using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;  // 添加配置命名空间
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NewsInsight.Api.Controllers
{
    [Route("api/proxy")]
    [ApiController]
    public class ProxyController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        // Inject HttpClient and IConfiguration into the controller
        public ProxyController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;  // Inject IConfiguration
        }

        // POST: api/proxy/getAnalysis
        [HttpPost("getAnalysis")]
        public async Task<IActionResult> GetAnalysis([FromBody] object requestData)
        {
            var apiUrl = "https://ark.cn-beijing.volces.com/api/v3/chat/completions";  // Target API URL

            // Get the API key from appsettings.json
            var volcengineAPIKey = _configuration["AppSettings:VolcengineAPIKey"];

            // If API key is missing, return an error
            if (string.IsNullOrEmpty(volcengineAPIKey))
            {
                return BadRequest("API Key is missing in the configuration.");
            }

            // Create HttpContent object for the request body
            var content = new StringContent(requestData.ToString(), Encoding.UTF8, "application/json");

            // Add Authorization Header (using Bearer Token)
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {volcengineAPIKey}");

            // Send POST request to the target API
            var response = await _httpClient.PostAsync(apiUrl, content);  // Send request with HttpContent

            // Check if the response is successful
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return Ok(result);  // Return result from the target API
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                return BadRequest($"Error calling external API: {errorMessage}");  // Return detailed error message
            }
        }
    }
}
