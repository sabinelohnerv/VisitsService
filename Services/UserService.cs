using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace VisitService.API.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> GetEmailByUserIdAsync(Guid userId)
        {
            var response = await _httpClient.GetAsync($"/user/{userId}/email");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadFromJsonAsync<EmailResponse>();
            return json?.Email;
        }

        private class EmailResponse
        {
            public string Email { get; set; } = string.Empty;
        }
    }
}
