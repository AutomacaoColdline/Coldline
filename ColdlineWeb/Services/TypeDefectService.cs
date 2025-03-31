using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using ColdlineWeb.Models;

namespace ColdlineWeb.Services
{
    public class TypeDefectService
    {
        private readonly HttpClient _http;

        public TypeDefectService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<TypeDefectModel>> GetAllAsync() =>
            await _http.GetFromJsonAsync<List<TypeDefectModel>>("api/TypeDefect") ?? new();

        public async Task<TypeDefectModel?> GetByIdAsync(string id) =>
            await _http.GetFromJsonAsync<TypeDefectModel>($"api/TypeDefect/{id}");

        public async Task<TypeDefectModel?> CreateAsync(TypeDefectModel typeDefect)
        {
            var response = await _http.PostAsJsonAsync("api/TypeDefect", typeDefect);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<TypeDefectModel>()
                : null;
        }

        public async Task<bool> UpdateAsync(string id, TypeDefectModel typeDefect)
        {
            var response = await _http.PutAsJsonAsync($"api/TypeDefect/{id}", typeDefect);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/TypeDefect/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
