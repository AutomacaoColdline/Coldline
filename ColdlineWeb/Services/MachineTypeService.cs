using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using ColdlineWeb.Models;

namespace ColdlineWeb.Services
{
    public class MachineTypeService
    {
        private readonly HttpClient _http;

        public MachineTypeService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<MachineType>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<MachineType>>("api/MachineType") ?? new();
        }

        public async Task<MachineType?> GetByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<MachineType>($"api/MachineType/{id}");
        }

        public async Task<MachineType?> CreateAsync(MachineType machineType)
        {
            var response = await _http.PostAsJsonAsync("api/MachineType", machineType);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<MachineType>()
                : null;
        }

        public async Task<bool> UpdateAsync(string id, MachineType machineType)
        {
            var response = await _http.PutAsJsonAsync($"api/MachineType/{id}", machineType);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/MachineType/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
