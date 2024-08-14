using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EventManagementServer.DTOs; // Correct namespace
using System.Net.Http.Headers;
using System.Net.Http.Formatting;

namespace EventManagementServer.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class AppliedUsersController : ControllerBase
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        [HttpPost]
        public async Task<IActionResult> FetchAppliedUsers([FromBody] AppliedUsersRequestDto request)
        {
            try
            {
                var clientApiKey = Request.Headers["X-API-Key"].FirstOrDefault();

                if (string.IsNullOrEmpty(clientApiKey))
                {
                    return BadRequest(new { error = "X-API-Key header is required" });
                }

                var url = "https://aks-cluster-prod.westus3.cloudapp.azure.com/AppliedUser/Search?includeInactive=false&page=1&pageSize=80";

                if (!string.IsNullOrEmpty(request.Keyword))
                {
                    url += $"&keyword={Uri.EscapeDataString(request.Keyword)}";
                }

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", clientApiKey);

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
