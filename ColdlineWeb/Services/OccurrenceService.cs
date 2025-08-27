using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;

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

        public async Task<List<OccurrenceModel>> SearchAsync(OccurrenceSearchFilter filter)
        {
            var qs = new List<string>();

            if (!string.IsNullOrWhiteSpace(filter.UserId))
                qs.Add($"UserId={Uri.EscapeDataString(filter.UserId)}");
            
            if (!string.IsNullOrWhiteSpace(filter.MachineID))
                qs.Add($"MachineID={Uri.EscapeDataString(filter.MachineID)}");

            if (filter.Finished.HasValue)
                qs.Add($"Finished={(filter.Finished.Value ? "true" : "false")}");

            if (!string.IsNullOrWhiteSpace(filter.OccurrenceTypeId))
                qs.Add($"OccurrenceTypeId={Uri.EscapeDataString(filter.OccurrenceTypeId)}");

            if (filter.StartDate.HasValue)
                qs.Add($"StartDate={Uri.EscapeDataString(filter.StartDate.Value.ToString("o"))}");

            if (filter.EndDate.HasValue)
                qs.Add($"EndDate={Uri.EscapeDataString(filter.EndDate.Value.ToString("o"))}");

            var url = "api/Occurrence/search" + (qs.Count > 0 ? "?" + string.Join("&", qs) : "");
            return await _http.GetFromJsonAsync<List<OccurrenceModel>>(url) ?? new();
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
