using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using ColdlineWeb.Util;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;
using ColdlineWeb.Models.Enum;

using System.Text.Json;


namespace ColdlineWeb.Services
{
    public class MachineService
    {
        private readonly HttpClient _http;

        public MachineService(HttpClient http)
        {
            _http = http;
        }

        /// <summary>
        /// Busca a lista de todas as máquinas.
        /// </summary>
        public async Task<List<MachineModel>> GetAllMachinesAsync()
        {
            return await _http.GetFromJsonAsync<List<MachineModel>>("api/Machine") ?? new List<MachineModel>();
        }

        /// <summary>
        /// Busca uma máquina pelo ID.
        /// </summary>
        public async Task<MachineModel?> GetMachineByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<MachineModel>($"api/Machine/{id}");
        }

        /// <summary>
        /// Cria uma nova máquina.
        /// </summary>
        public async Task<MachineModel?> CreateMachineAsync(MachineModel machine)
        {
            var response = await _http.PostAsJsonAsync("api/Machine", machine);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<MachineModel>() : null;
        }

        /// <summary>
        /// Atualiza uma máquina pelo ID.
        /// </summary>
        public async Task<bool> UpdateMachineAsync(string id, MachineModel machine)
        {
            var response = await _http.PutAsJsonAsync($"api/Machine/{id}", machine);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Deleta uma máquina pelo ID.
        /// </summary>
        public async Task<bool> DeleteMachineAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/Machine/{id}");
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Busca máquinas com base em filtros.
        /// </summary>
        public async Task<List<MachineModel>> SearchMachinesAsync(MachineFilterModel filter)
        {
            var query = filter.ToQueryString();
            var response = await _http.GetAsync($"api/Machine/search?{query}");

            if (!response.IsSuccessStatusCode)
                return new List<MachineModel>();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var itemsElement = doc.RootElement.GetProperty("items");

            var items = JsonSerializer.Deserialize<List<MachineModel>>(itemsElement.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return items ?? new List<MachineModel>();
        }
        public async Task<bool> FinalizeMachineAsync(string id)
        {
            // Busca a máquina atual para não sobrescrever campos
            var machine = await GetMachineByIdAsync(id);
            if (machine == null)
                return false;

            machine.Status = MachineStatus.Finished;
            machine.Process = null;
            machine.Monitoring = null;
            machine.Quality = null;

            var response = await _http.PutAsJsonAsync($"api/Machine/{id}", machine);
            return response.IsSuccessStatusCode;
        }



    }
}
