using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using ColdlineWeb.Models;

namespace ColdlineWeb.Services
{
    public class DepartmentService
    {
        private readonly HttpClient _http;

        public DepartmentService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<DepartmentModel>> GetAllAsync() =>
            await _http.GetFromJsonAsync<List<DepartmentModel>>("api/Department") ?? new();

        public async Task<DepartmentModel?> GetByIdAsync(string id) =>
            await _http.GetFromJsonAsync<DepartmentModel>($"api/Department/{id}");

        public async Task<DepartmentModel?> CreateAsync(DepartmentModel department)
        {
            var response = await _http.PostAsJsonAsync("api/Department", department);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<DepartmentModel>()
                : null;
        }

        public async Task<bool> UpdateAsync(string id, DepartmentModel department)
        {
            var response = await _http.PutAsJsonAsync($"api/Department/{id}", department);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/Department/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
