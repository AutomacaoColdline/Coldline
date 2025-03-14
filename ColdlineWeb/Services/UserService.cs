using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Forms;
using ColdlineWeb.Models;

namespace ColdlineWeb.Services
{
    public class UserService
    {
        private readonly HttpClient _http;

        public UserService(HttpClient http)
        {
            _http = http;
        }

        /// <summary>
        /// Busca a lista de usuários cadastrados.
        /// </summary>
        public async Task<List<UserModel>> GetUsersAsync()
        {
            return await _http.GetFromJsonAsync<List<UserModel>>("api/User") ?? new List<UserModel>();
        }

        /// <summary>
        /// Busca a lista de tipos de usuários.
        /// </summary>
        public async Task<List<ReferenceEntity>> GetUserTypesAsync()
        {
            return await _http.GetFromJsonAsync<List<ReferenceEntity>>("api/UserType") ?? new List<ReferenceEntity>();
        }

        /// <summary>
        /// Busca a lista de departamentos.
        /// </summary>
        public async Task<List<ReferenceEntity>> GetDepartmentsAsync()
        {
            return await _http.GetFromJsonAsync<List<ReferenceEntity>>("api/Department") ?? new List<ReferenceEntity>();
        }

        /// <summary>
        /// Salva um usuário novo ou atualiza um existente.
        /// </summary>
        public async Task<bool> SaveUserAsync(UserModel user)
        {
            var response = await _http.PostAsJsonAsync("api/User", user);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Exclui um usuário com base no ID.
        /// </summary>
        public async Task<bool> DeleteUserAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/User/{id}");
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Faz o upload da imagem do usuário e retorna o nome do arquivo salvo.
        /// </summary>
        public async Task<string?> UploadImageAsync(IBrowserFile file, string userName)
        {
            if (file == null)
                return null;

            var fileName = $"{userName}.png";
            var uploadUrl = $"http://10.0.0.44:5000/api/User/upload-image?fileName={fileName}";

            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(file.OpenReadStream()), "file", fileName);

            var response = await _http.PostAsync(uploadUrl, content);
            return response.IsSuccessStatusCode ? fileName : null;
        }

        /// <summary>
        /// Busca um usuário pelo número de identificação.
        /// </summary>
        public async Task<UserModel?> GetUserByIdentificationAsync(string identificationNumber)
        {
            return await _http.GetFromJsonAsync<UserModel>($"api/User/identification/{identificationNumber}");
        }

        /// <summary>
        /// Atualiza um usuário com base no ID.
        /// </summary>
        public async Task<bool> UpdateUserAsync(string id, UserModel user)
        {
            var response = await _http.PutAsJsonAsync($"api/User/{id}", user);
            return response.IsSuccessStatusCode;
        }
    }
}
