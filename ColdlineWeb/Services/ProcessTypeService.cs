using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using ColdlineWeb.Models;

namespace ColdlineWeb.Services
{
    public class ProcessTypeService
    {
        private readonly HttpClient _http;

        public ProcessTypeService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ProcessType>> GetAllAsync() =>
            await _http.GetFromJsonAsync<List<ProcessType>>("api/ProcessType") ?? new();

        public async Task<ProcessType?> GetByIdAsync(string id) =>
            await _http.GetFromJsonAsync<ProcessType>($"api/ProcessType/{id}");

        public async Task<ProcessType?> CreateAsync(ProcessType processType)
        {
            var response = await _http.PostAsJsonAsync("api/ProcessType", processType);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<ProcessType>()
                : null;
        }

        public async Task<bool> UpdateAsync(string id, ProcessType processType)
        {
            var response = await _http.PutAsJsonAsync($"api/ProcessType/{id}", processType);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/ProcessType/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
