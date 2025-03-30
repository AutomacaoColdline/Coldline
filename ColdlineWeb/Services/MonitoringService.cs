using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using ColdlineWeb.Models;

namespace ColdlineWeb.Services
{
    public class MonitoringService
    {
        private readonly HttpClient _http;

        public MonitoringService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Monitoring>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<Monitoring>>("api/Monitoring") ?? new();
        }

        public async Task<Monitoring?> GetByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<Monitoring>($"api/Monitoring/{id}");
        }

        public async Task<Monitoring?> CreateAsync(Monitoring monitoring)
        {
            var response = await _http.PostAsJsonAsync("api/Monitoring", monitoring);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<Monitoring>()
                : null;
        }

        public async Task<bool> UpdateAsync(string id, Monitoring monitoring)
        {
            var response = await _http.PutAsJsonAsync($"api/Monitoring/{id}", monitoring);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/Monitoring/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
