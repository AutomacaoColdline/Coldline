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

        public async Task<List<ProcessTypeModel>> GetAllAsync() =>
            await _http.GetFromJsonAsync<List<ProcessTypeModel>>("api/ProcessType") ?? new();

        public async Task<ProcessTypeModel?> GetByIdAsync(string id) =>
            await _http.GetFromJsonAsync<ProcessTypeModel>($"api/ProcessType/{id}");

        public async Task<ProcessTypeModel?> CreateAsync(ProcessTypeModel processType)
        {
            var response = await _http.PostAsJsonAsync("api/ProcessType", processType);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<ProcessTypeModel>()
                : null;
        }

        public async Task<bool> UpdateAsync(string id, ProcessTypeModel processType)
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
