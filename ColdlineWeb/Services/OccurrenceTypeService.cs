using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;

namespace ColdlineWeb.Services
{
    public class OccurrenceTypeService
    {
        private readonly HttpClient _http;

        public OccurrenceTypeService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<OccurrenceTypeModel>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<OccurrenceTypeModel>>("api/OccurrenceType") ?? new();
        }

        public async Task<OccurrenceTypeModel?> GetByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<OccurrenceTypeModel>($"api/OccurrenceType/{id}");
        }

        public async Task<OccurrenceTypeModel?> CreateAsync(OccurrenceTypeModel occurrenceType)
        {
            var response = await _http.PostAsJsonAsync("api/OccurrenceType", occurrenceType);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<OccurrenceTypeModel>()
                : null;
        }

        public async Task<bool> UpdateAsync(string id, OccurrenceTypeModel occurrenceType)
        {
            var response = await _http.PutAsJsonAsync($"api/OccurrenceType/{id}", occurrenceType);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/OccurrenceType/{id}");
            return response.IsSuccessStatusCode;
        }
        public async Task<List<OccurrenceTypeModel>> SearchAsync(OccurrenceTypeFilterModel? filter = null)
        {
            string url = "api/OccurrenceType/search";

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

            return await _http.GetFromJsonAsync<List<OccurrenceTypeModel>>(url) ?? new();
        }
    }
}
