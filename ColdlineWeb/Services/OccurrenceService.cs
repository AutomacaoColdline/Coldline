using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using ColdlineWeb.Models;

namespace ColdlineWeb.Services
{
    public class OccurrenceService
    {
        private readonly HttpClient _http;

        public OccurrenceService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Occurrence>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<Occurrence>>("api/Occurrence") ?? new();
        }

        public async Task<Occurrence?> GetByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<Occurrence>($"api/Occurrence/{id}");
        }

        public async Task<Occurrence?> CreateAsync(Occurrence occurrence)
        {
            var response = await _http.PostAsJsonAsync("api/Occurrence", occurrence);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<Occurrence>()
                : null;
        }

        public async Task<bool> UpdateAsync(string id, Occurrence occurrence)
        {
            var response = await _http.PutAsJsonAsync($"api/Occurrence/{id}", occurrence);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/Occurrence/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<Occurrence?> StartOccurrenceAsync(StartOccurrenceRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/Occurrence/start-occurrence", request);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<Occurrence>()
                : null;
        }

        public async Task<bool> EndOccurrenceAsync(string id)
        {
            var response = await _http.PostAsync($"api/Occurrence/end-occurrence/{id}", null);
            return response.IsSuccessStatusCode;
        }
    }
}
