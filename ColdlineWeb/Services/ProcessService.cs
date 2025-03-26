using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;

namespace ColdlineWeb.Services
{
    public class ProcessService
    {
        private readonly HttpClient _http;

        public ProcessService(HttpClient http)
        {
            _http = http;
        }
        
        public async Task<ProcessStatisticsDto?> GetProcessStatisticsAsync(string processId, string processTypeId)
        {
            return await _http.GetFromJsonAsync<ProcessStatisticsDto>($"api/Process/statistics/{processId}/{processTypeId}");
        }

        public async Task<List<ProcessModel>> GetAllProcessesAsync()
        {
            return await _http.GetFromJsonAsync<List<ProcessModel>>("api/Process") ?? new List<ProcessModel>();
        }

        public async Task<ProcessModel?> GetProcessByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<ProcessModel>($"api/Process/{id}");
        }

        public async Task<ProcessModel?> CreateProcessAsync(ProcessModel process)
        {
            var response = await _http.PostAsJsonAsync("api/Process", process);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ProcessModel>() : null;
        }

        public async Task<bool> UpdateProcessAsync(string id, ProcessModel process)
        {
            var response = await _http.PutAsJsonAsync($"api/Process/{id}", process);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteProcessAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/Process/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<ProcessModel>> SearchProcessesAsync(ProcessFilterModel filter)
        {
            var response = await _http.PostAsJsonAsync("api/Process/search", filter);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<List<ProcessModel>>() ?? new List<ProcessModel>()
                : new List<ProcessModel>();
        }

        public async Task<ProcessModel?> StartProcessAsync(StartProcessRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/Process/start-process", request);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<ProcessModel>()
                : null;
        }

        public async Task<bool> EndProcessAsync(string processId)
        {
            var response = await _http.PostAsync($"api/Process/end-process/{processId}", null);
            return response.IsSuccessStatusCode;
        }
    }
}
