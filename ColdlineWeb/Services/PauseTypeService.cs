using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using ColdlineWeb.Models;

namespace ColdlineWeb.Services
{
    public class PauseTypeService
    {
        private readonly HttpClient _http;

        public PauseTypeService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<PauseTypeModel>> GetAllAsync() =>
            await _http.GetFromJsonAsync<List<PauseTypeModel>>("api/PauseType") ?? new();

        public async Task<PauseTypeModel?> GetByIdAsync(string id) =>
            await _http.GetFromJsonAsync<PauseTypeModel>($"api/PauseType/{id}");

        public async Task<PauseTypeModel?> CreateAsync(PauseTypeModel pauseType)
        {
            var response = await _http.PostAsJsonAsync("api/PauseType", pauseType);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<PauseTypeModel>()
                : null;
        }

        public async Task<bool> UpdateAsync(string id, PauseTypeModel pauseType)
        {
            var response = await _http.PutAsJsonAsync($"api/PauseType/{id}", pauseType);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/PauseType/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
