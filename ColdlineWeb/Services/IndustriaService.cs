using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using ColdlineWeb.Models;

namespace ColdlineWeb.Services
{
    public class IndustriaService
    {
        private readonly HttpClient _http;

        public IndustriaService(HttpClient http)
        {
            _http = http;
        }

        public async Task<UserModel?> GetUserById(string id) =>
            await _http.GetFromJsonAsync<UserModel>($"api/User/identification/{id}");

        public async Task<ProcessModel?> GetProcessById(string processId)
        {
            try
            {
                return await _http.GetFromJsonAsync<ProcessModel>($"api/Process/{processId}");
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        // 🔥 Adicionando os métodos que estavam faltando:
        public async Task<List<ReferenceEntity>> GetProcessTypesAsync()
        {
            return await _http.GetFromJsonAsync<List<ReferenceEntity>>("api/ProcessType") ?? new List<ReferenceEntity>();
        }

        public async Task<List<MachineModel>> GetMachinesAsync()
        {
            return await _http.GetFromJsonAsync<List<MachineModel>>("api/Machine") ?? new List<MachineModel>();
        }

        public async Task<bool> StartProcessAsync(string identificationNumber, string processTypeId, string machineId, bool preIndustrialization)
        {
            var request = new
            {
                IdentificationNumber = identificationNumber,
                ProcessTypeId = processTypeId,
                MachineId = machineId,
                PreIndustrialization = preIndustrialization
            };

            var response = await _http.PostAsJsonAsync("api/Process/start-process", request);
            return response.IsSuccessStatusCode;
        }
        public async Task<bool> StartProcessAsync(string identificationNumber, string processTypeId, bool preIndustrialization, string? machineId = null)
        {
            var request = new
            {
                IdentificationNumber = identificationNumber,
                ProcessTypeId = processTypeId,
                PreIndustrialization = preIndustrialization,
                MachineId = machineId // Será enviado apenas se não for null
            };

            var response = await _http.PostAsJsonAsync("api/Process/start-process", request);
            return response.IsSuccessStatusCode;
        }

    }
}
