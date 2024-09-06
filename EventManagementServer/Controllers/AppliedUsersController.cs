using Microsoft.AspNetCore.Mvc;
using EventManagementServer.DTOs; // Correct namespace
using System.Net.Http.Formatting;
using Microsoft.AspNetCore.Authorization;

namespace EventManagementServer.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    [Authorize]
    public class AppliedUsersController : ControllerBase
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string _apiKey = "GM4WMMBVGJRWGLJUGUZWELJUG5RWKLLBHEYDMLJXG5QWEMTFGM4WCZDDHE";  // Your API key

        public AppliedUsersController(IConfiguration configuration)
        {
            // The API key is hardcoded in this example
        }

        [HttpPost]
        public async Task<IActionResult> FetchAppliedUsers([FromBody] AppliedUsersRequestDto request)
        {
            try
            {
                // Base URL for the API request
                var url = "https://aks-cluster-prod.westus3.cloudapp.azure.com/AppliedUser/Search?includeInactive=false&page=1&pageSize=80";

                // Add keyword if provided
                if (!string.IsNullOrEmpty(request.Keyword))
                {
                    url += $"&keyword={Uri.EscapeDataString(request.Keyword)}";
                }

                // Create the HttpRequestMessage
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);

                // Add the API key in the headers
                httpRequestMessage.Headers.Add("X-API-Key", _apiKey);  // API key added in the headers

                // Send the GET request
                var response = await _httpClient.SendAsync(httpRequestMessage);

                // Check if the response is successful
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, new { error = response.ReasonPhrase });
                }

                // Read the response content
                var data = await response.Content.ReadAsAsync<AppliedUsersResponseDto>(new[] { new JsonMediaTypeFormatter() });

                // Return only the list of users
                return Ok(data.Results);
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
