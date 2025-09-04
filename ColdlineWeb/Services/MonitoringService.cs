using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;                 // QueryHelpers
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;

namespace ColdlineWeb.Services
{
    public class MonitoringService
    {
        private readonly HttpClient _http;

        public MonitoringService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<MonitoringModel>> GetAllAsync()
            => await _http.GetFromJsonAsync<List<MonitoringModel>>("api/Monitoring") ?? new();

        public async Task<MonitoringModel?> GetByIdAsync(string id)
            => await _http.GetFromJsonAsync<MonitoringModel>($"api/Monitoring/{id}");

        // ⚠️ Remova ou marque como obsolete o método antigo FilterAsync (que retornava List)
        // [Obsolete("Use SearchAsync() que retorna PagedResult.")]
        // public async Task<List<MonitoringModel>> FilterAsync(MonitoringFilterModel filter) { ... }

        // ✅ Novo: busca paginada via GET /api/Monitoring/search
        public async Task<ColdlineWeb.Models.Common.PagedResult<MonitoringModel>> SearchAsync(MonitoringFilterModel filter)
        {
            var query = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(filter.Estado))          query["Estado"] = filter.Estado!;
            if (!string.IsNullOrWhiteSpace(filter.Cidade))          query["Cidade"] = filter.Cidade!;
            if (!string.IsNullOrWhiteSpace(filter.Identificador))   query["Identificador"] = filter.Identificador!;
            if (!string.IsNullOrWhiteSpace(filter.Unidade))         query["Unidade"] = filter.Unidade!;
            if (!string.IsNullOrWhiteSpace(filter.MonitoringTypeId))query["MonitoringTypeId"] = filter.MonitoringTypeId!;

            query["Page"] = (filter.Page <= 0 ? 1 : filter.Page).ToString();
            query["PageSize"] = (filter.PageSize <= 0 ? 20 : filter.PageSize).ToString();

            var url = QueryHelpers.AddQueryString("api/Monitoring/search", query);

            var result = await _http.GetFromJsonAsync<ColdlineWeb.Models.Common.PagedResult<MonitoringModel>>(url);
            return result ?? new ColdlineWeb.Models.Common.PagedResult<MonitoringModel>
            {
                Items = System.Array.Empty<MonitoringModel>(),
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<MonitoringModel?> CreateAsync(MonitoringModel monitoring)
        {
            var response = await _http.PostAsJsonAsync("api/Monitoring", monitoring);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<MonitoringModel>()
                : null;
        }

        public async Task<bool> UpdateAsync(string id, MonitoringModel monitoring)
        {
            var response = await _http.PutAsJsonAsync($"api/Monitoring/{id}", monitoring);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/Monitoring/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
