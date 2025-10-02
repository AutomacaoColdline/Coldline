using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;
using ColdlineWeb.Models.Common;

namespace ColdlineWeb.Services
{
    public class ProcessTypeService
    {
        private readonly HttpClient _http;

        public ProcessTypeService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ProcessTypeModel>> GetAllAsync() =>
            await _http.GetFromJsonAsync<List<ProcessTypeModel>>("api/ProcessType") ?? new();

        public async Task<ProcessTypeModel?> GetByIdAsync(string id) =>
            await _http.GetFromJsonAsync<ProcessTypeModel>($"api/ProcessType/{id}");

        public async Task<ProcessTypeModel?> CreateAsync(ProcessTypeModel processType)
        {
            var response = await _http.PostAsJsonAsync("api/ProcessType", processType);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<ProcessTypeModel>()
                : null;
        }

        public async Task<bool> UpdateAsync(string id, ProcessTypeModel processType)
        {
            var response = await _http.PutAsJsonAsync($"api/ProcessType/{id}", processType);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/ProcessType/{id}");
            return response.IsSuccessStatusCode;
        }

        // 🔹 Corrigido para retornar PagedResult<ProcessTypeModel>
        public async Task<PagedResult<ProcessTypeModel>> SearchAsync(ProcessTypeFilterModel filter)
        {
            // montar query string manualmente (adicione as propriedades que precisar)
            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(filter.Name))
                queryParams.Add($"name={Uri.EscapeDataString(filter.Name)}");
            // exemplo: queryParams.Add($"status={Uri.EscapeDataString(filter.Status)}");
            queryParams.Add($"page={filter.Page}");
            queryParams.Add($"pageSize={filter.PageSize}");

            var url = "api/ProcessType/search" + (queryParams.Any() ? "?" + string.Join("&", queryParams) : string.Empty);

            var response = await _http.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return new PagedResult<ProcessTypeModel>
                {
                    Items = new List<ProcessTypeModel>(),
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };
            }

            var result = await response.Content.ReadFromJsonAsync<PagedResult<ProcessTypeModel>>();
            return result ?? new PagedResult<ProcessTypeModel>
            {
                Items = new List<ProcessTypeModel>(),
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }
    }
}
