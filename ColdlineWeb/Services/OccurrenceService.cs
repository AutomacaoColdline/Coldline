using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;
using ColdlineWeb.Models.Common;
using Microsoft.AspNetCore.WebUtilities; // necessário para QueryHelpers

namespace ColdlineWeb.Services
{
    public class OccurrenceService
    {
        private readonly HttpClient _http;

        public OccurrenceService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<OccurrenceModel>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<OccurrenceModel>>("api/Occurrence") ?? new();
        }

        public async Task<PagedResult<OccurrenceModel>> SearchAsync(OccurrenceSearchFilter filter)
        {
            var query = new Dictionary<string, string?>();

            if (!string.IsNullOrWhiteSpace(filter.UserId))
                query["userId"] = filter.UserId;

            if (!string.IsNullOrWhiteSpace(filter.MachineID))
                query["machineId"] = filter.MachineID;

            if (filter.Finished.HasValue)
                query["finished"] = filter.Finished.Value.ToString().ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(filter.OccurrenceTypeId))
                query["occurrenceTypeId"] = filter.OccurrenceTypeId;

            if (filter.StartDate.HasValue)
                query["startDate"] = filter.StartDate.Value.ToString("o");

            if (filter.EndDate.HasValue)
                query["endDate"] = filter.EndDate.Value.ToString("o");

            // paginação
            query["page"] = (filter.Page ?? 1).ToString();
            query["pageSize"] = (filter.PageSize ?? 10).ToString();

            // ordenação
            if (!string.IsNullOrWhiteSpace(filter.SortBy))
                query["sortBy"] = filter.SortBy;
            query["sortDesc"] = (filter.SortDesc ?? false).ToString().ToLowerInvariant();

            var url = QueryHelpers.AddQueryString("api/Occurrence/search", query);

            var result = await _http.GetFromJsonAsync<PagedResult<OccurrenceModel>>(url);
            return result ?? new PagedResult<OccurrenceModel>
            {
                Items = Array.Empty<OccurrenceModel>(),
                Page = filter.Page ?? 1,
                PageSize = filter.PageSize ?? 10
            };
        }

        public async Task<OccurrenceModel?> GetByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<OccurrenceModel>($"api/Occurrence/{id}");
        }

        public async Task<OccurrenceModel?> CreateAsync(OccurrenceModel occurrence)
        {
            var response = await _http.PostAsJsonAsync("api/Occurrence", occurrence);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<OccurrenceModel>()
                : null;
        }

        public async Task<bool> UpdateAsync(string id, OccurrenceModel occurrence)
        {
            var response = await _http.PutAsJsonAsync($"api/Occurrence/{id}", occurrence);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/Occurrence/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<OccurrenceModel?> StartOccurrenceAsync(StartOccurrenceModel request)
        {
            var response = await _http.PostAsJsonAsync("api/Occurrence/start-occurrence", request);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<OccurrenceModel>()
                : null;
        }

        public async Task<bool> EndOccurrenceAsync(string id)
        {
            var response = await _http.PostAsync($"api/Occurrence/end-occurrence/{id}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> FinalizeOccurrenceAsync(string id)
            => (await _http.PostAsync($"api/Occurrence/finalize/{id}", null)).IsSuccessStatusCode;
    }
}
