using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using ColdlineWeb.Models;

namespace ColdlineWeb.Services
{
    public class DefectService
    {
        private readonly HttpClient _http;

        public DefectService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<DefectModel>> GetAllAsync() =>
            await _http.GetFromJsonAsync<List<DefectModel>>("api/Defect") ?? new();

        public async Task<DefectModel?> GetByIdAsync(string id) =>
            await _http.GetFromJsonAsync<DefectModel>($"api/Defect/{id}");

        public async Task<DefectModel?> CreateAsync(DefectModel defect)
        {
            var response = await _http.PostAsJsonAsync("api/Defect", defect);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<DefectModel>()
                : null;
        }

        public async Task<bool> UpdateAsync(string id, DefectModel defect)
        {
            var response = await _http.PutAsJsonAsync($"api/Defect/{id}", defect);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/Defect/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
