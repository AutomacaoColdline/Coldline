using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;
using ColdlineWeb.Models.Common;
using Microsoft.AspNetCore.WebUtilities;

namespace ColdlineWeb.Services
{
    public class OccurrenceTypeService
    {
        private readonly HttpClient _http;

        public OccurrenceTypeService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<OccurrenceTypeModel>> GetAllAsync() =>
            await _http.GetFromJsonAsync<List<OccurrenceTypeModel>>("api/OccurrenceType") ?? new();

        public async Task<OccurrenceTypeModel?> GetByIdAsync(string id) =>
            await _http.GetFromJsonAsync<OccurrenceTypeModel>($"api/OccurrenceType/{id}");

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

        // Versão paginada, seguindo padrão do UserTypeService
        public async Task<PagedResult<OccurrenceTypeModel>> SearchAsync(OccurrenceTypeFilterModel? filter = null)
        {
            var query = new Dictionary<string, string?>();

            if (filter is not null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Name))
                    query["name"] = filter.Name;

                if (!string.IsNullOrWhiteSpace(filter.DepartmentId))
                    query["departmentId"] = filter.DepartmentId;

                query["page"] = filter.Page > 0 ? filter.Page.ToString() : "1";
                query["pageSize"] = filter.PageSize > 0 ? filter.PageSize.ToString() : "10";

                if (!string.IsNullOrWhiteSpace(filter.SortBy))
                    query["sortBy"] = filter.SortBy;

                query["sortDesc"] = filter.SortDesc.ToString().ToLower();
            }

            var url = QueryHelpers.AddQueryString("api/OccurrenceType/search", query);

            var result = await _http.GetFromJsonAsync<PagedResult<OccurrenceTypeModel>>(url);

            return result ?? new PagedResult<OccurrenceTypeModel>
            {
                Items = Array.Empty<OccurrenceTypeModel>(),
                Page = filter?.Page ?? 1,
                PageSize = filter?.PageSize ?? 10
            };
        }
    }
}
