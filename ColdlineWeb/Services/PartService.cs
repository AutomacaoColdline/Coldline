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

        public async Task<List<Part>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<Part>>("api/Part") ?? new();
        }

        public async Task<Part?> GetByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<Part>($"api/Part/{id}");
        }

        public async Task<Part?> CreateAsync(Part part)
        {
            var response = await _http.PostAsJsonAsync("api/Part", part);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<Part>()
                : null;
        }

        public async Task<bool> UpdateAsync(string id, Part part)
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
