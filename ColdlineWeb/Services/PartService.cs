using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using ColdlineWeb.Models;

namespace ColdlineWeb.Services
{
    public class PartService
    {
        private readonly HttpClient _http;

        public PartService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<PartModel>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<PartModel>>("api/Part") ?? new();
        }

        public async Task<PartModel?> GetByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<PartModel>($"api/Part/{id}");
        }

        public async Task<PartModel?> CreateAsync(PartModel part)
        {
            var response = await _http.PostAsJsonAsync("api/Part", part);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<PartModel>()
                : null;
        }

        public async Task<bool> UpdateAsync(string id, PartModel part)
        {
            var response = await _http.PutAsJsonAsync($"api/Part/{id}", part);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/Part/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
