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

        public async Task<List<Department>> GetAllAsync() =>
            await _http.GetFromJsonAsync<List<Department>>("api/Department") ?? new();

        public async Task<Department?> GetByIdAsync(string id) =>
            await _http.GetFromJsonAsync<Department>($"api/Department/{id}");

        public async Task<Department?> CreateAsync(Department department)
        {
            var response = await _http.PostAsJsonAsync("api/Department", department);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<Department>()
                : null;
        }

        public async Task<bool> UpdateAsync(string id, Department department)
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
