using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using System.Text.Json.Nodes;

namespace VisitService.API.Services
{
    public class PropertyService
    {
        private readonly HttpClient _httpClient;

        public PropertyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> GetPropertyTitleAsync(Guid idProperty)
        {
            var query = new
            {
                query = @"
                    query ($id: UUID!) {
                        getPropertyById(id: $id) {
                            title
                        }
                    }",
                variables = new { id = idProperty }
            };

            var response = await _httpClient.PostAsync(
                "/graphql",
                new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadFromJsonAsync<JsonObject>();
            return json?["data"]?["getPropertyById"]?["title"]?.ToString();
        }
    }
}
