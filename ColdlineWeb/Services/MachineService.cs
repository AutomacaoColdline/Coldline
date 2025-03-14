using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;

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
            var response = await _http.PostAsJsonAsync("api/Machine/search", filter);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<List<MachineModel>>() ?? new List<MachineModel>() : new List<MachineModel>();
        }
    }
}
