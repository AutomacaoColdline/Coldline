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

        public async Task<List<Defect>> GetAllAsync() =>
            await _http.GetFromJsonAsync<List<Defect>>("api/Defect") ?? new();

        public async Task<Defect?> GetByIdAsync(string id) =>
            await _http.GetFromJsonAsync<Defect>($"api/Defect/{id}");

        public async Task<Defect?> CreateAsync(Defect defect)
        {
            var response = await _http.PostAsJsonAsync("api/Defect", defect);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<Defect>()
                : null;
        }

        public async Task<bool> UpdateAsync(string id, Defect defect)
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
