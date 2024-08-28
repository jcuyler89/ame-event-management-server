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
        private readonly string _apiKey;

        public AppliedUsersController(IConfiguration configuration)
        {
            _apiKey = configuration["ApiSettings:ApiKey"];

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
        }

        [HttpPost]
        public async Task<IActionResult> FetchAppliedUsers([FromBody] AppliedUsersRequestDto request)
        {
            try
            {
                var url = "https://aks-cluster-prod.westus3.cloudapp.azure.com/AppliedUser/Search?includeInactive=false&page=1&pageSize=80";

                if (!string.IsNullOrEmpty(request.Keyword))
                {
                    url += $"&keyword={Uri.EscapeDataString(request.Keyword)}";
                }

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, new { error = response.ReasonPhrase });
                }

                var data = await response.Content.ReadAsAsync<AppliedUsersResponseDto>(new[] { new JsonMediaTypeFormatter() });

                return Ok(data.Results); // Return only the list of users
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
