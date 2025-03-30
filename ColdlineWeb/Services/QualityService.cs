using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;

namespace ColdlineWeb.Services
{
    public class QualityService
    {
        private readonly HttpClient _http;

        public QualityService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Quality>> GetAllAsync() =>
            await _http.GetFromJsonAsync<List<Quality>>("api/Quality") ?? new();

        public async Task<Quality?> GetByIdAsync(string id) =>
            await _http.GetFromJsonAsync<Quality>($"api/Quality/{id}");

        public async Task<Quality?> CreateAsync(Quality quality)
        {
            var response = await _http.PostAsJsonAsync("api/Quality", quality);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<Quality>()
                : null;
        }

        public async Task<bool> UpdateAsync(string id, Quality quality)
        {
            var response = await _http.PutAsJsonAsync($"api/Quality/{id}", quality);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/Quality/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<Quality>> SearchAsync(QualityFilter filter)
        {
            var response = await _http.PostAsJsonAsync("api/Quality/search", filter);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<List<Quality>>() ?? new()
                : new();
        }
    }
}
