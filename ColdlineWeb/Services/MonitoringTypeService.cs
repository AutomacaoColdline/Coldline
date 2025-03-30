using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using ColdlineWeb.Models;

namespace ColdlineWeb.Services
{
    public class MonitoringTypeService
    {
        private readonly HttpClient _http;

        public MonitoringTypeService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<MonitoringType>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<MonitoringType>>("api/MonitoringType") ?? new();
        }

        public async Task<MonitoringType?> GetByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<MonitoringType>($"api/MonitoringType/{id}");
        }

        public async Task<MonitoringType?> CreateAsync(MonitoringType monitoringType)
        {
            var response = await _http.PostAsJsonAsync("api/MonitoringType", monitoringType);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<MonitoringType>()
                : null;
        }

        public async Task<bool> UpdateAsync(string id, MonitoringType monitoringType)
        {
            var response = await _http.PutAsJsonAsync($"api/MonitoringType/{id}", monitoringType);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/MonitoringType/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
