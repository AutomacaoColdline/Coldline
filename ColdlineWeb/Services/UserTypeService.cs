using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;
using ColdlineWeb.Models.Common;
using Microsoft.AspNetCore.WebUtilities;

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

        public async Task<PagedResult<UserTypeModel>> SearchAsync(UserTypeFilterModel? filter = null)
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

            var url = QueryHelpers.AddQueryString("api/UserType/search", query);

            var result = await _http.GetFromJsonAsync<PagedResult<UserTypeModel>>(url);

            return result ?? new PagedResult<UserTypeModel>
            {
                Items = new List<UserTypeModel>(),
                Page = filter?.Page ?? 1,
                PageSize = filter?.PageSize ?? 10,
                Total = 0
            };
        }
    }
}
