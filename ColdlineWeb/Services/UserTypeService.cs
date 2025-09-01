using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;

namespace ColdlineWeb.Services
{
    public class UserTypeService
    {
        private readonly HttpClient _http;

        public UserTypeService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<UserTypeModel>> GetAllAsync() =>
            await _http.GetFromJsonAsync<List<UserTypeModel>>("api/UserType") ?? new();

        public async Task<UserTypeModel?> GetByIdAsync(string id) =>
            await _http.GetFromJsonAsync<UserTypeModel>($"api/UserType/{id}");

        public async Task<UserTypeModel?> CreateAsync(UserTypeModel userType)
        {
            var response = await _http.PostAsJsonAsync("api/UserType", userType);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<UserTypeModel>()
                : null;
        }

        public async Task<bool> UpdateAsync(string id, UserTypeModel userType)
        {
            var response = await _http.PutAsJsonAsync($"api/UserType/{id}", userType);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/UserType/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<UserTypeModel>> SearchAsync(UserTypeFilterModel? filter = null)
        {
            string url = "api/UserType/search";

            if (filter is not null)
            {
                var qs = new List<string>();

                if (!string.IsNullOrWhiteSpace(filter.Name))
                    qs.Add($"name={Uri.EscapeDataString(filter.Name)}");

                if (!string.IsNullOrWhiteSpace(filter.DepartmentId))
                    qs.Add($"departmentId={Uri.EscapeDataString(filter.DepartmentId)}");

                if (qs.Count > 0)
                    url += "?" + string.Join("&", qs);
            }

            return await _http.GetFromJsonAsync<List<UserTypeModel>>(url) ?? new();
        }
    }
}
