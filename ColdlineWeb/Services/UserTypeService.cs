using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using ColdlineWeb.Models;

namespace ColdlineWeb.Services
{
    public class UserTypeService
    {
        private readonly HttpClient _http;

        public UserTypeService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<UserType>> GetAllAsync() =>
            await _http.GetFromJsonAsync<List<UserType>>("api/UserType") ?? new();

        public async Task<UserType?> GetByIdAsync(string id) =>
            await _http.GetFromJsonAsync<UserType>($"api/UserType/{id}");

        public async Task<UserType?> CreateAsync(UserType userType)
        {
            var response = await _http.PostAsJsonAsync("api/UserType", userType);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<UserType>()
                : null;
        }

        public async Task<bool> UpdateAsync(string id, UserType userType)
        {
            var response = await _http.PutAsJsonAsync($"api/UserType/{id}", userType);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/UserType/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
