using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.WebUtilities;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Common; 
using ColdlineWeb.Models.Filter;

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
        public async Task<string?> UploadImageAsync(IBrowserFile file, string oldFileName, string newFileName)
        {
            if (file == null)
            {
                Console.WriteLine("⚠ Nenhum arquivo foi selecionado para upload.");
                return null;
            }

            long maxFileSize = 20 * 1024 * 1024; // 20MB

            try
            {
                using var stream = file.OpenReadStream(maxFileSize);

                var uploadUrl = $"http://10.0.0.44:4000/api/User/upload-image?oldFileName={oldFileName}&newFileName={newFileName}";

                var content = new MultipartFormDataContent();
                content.Add(new StreamContent(stream), "file", newFileName);

                var response = await _http.PostAsync(uploadUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"✅ Upload bem-sucedido: {newFileName}");
                    return newFileName;
                }
                else
                {
                    Console.WriteLine($"❌ Falha no upload. Código: {response.StatusCode}");
                    return null;
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"❌ Erro: O arquivo excede o tamanho permitido de {maxFileSize / 1024 / 1024}MB. Detalhes: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Faz busca paginada de usuários com filtros opcionais de nome, email, departamento e tipo de usuário.
        /// </summary>
        public async Task<PagedResult<UserModel>> SearchUsersAsync(UserFilterModel filter)
        {
            var query = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(filter.Name))
                query["name"] = filter.Name;

            if (!string.IsNullOrWhiteSpace(filter.Email))
                query["email"] = filter.Email;

            if (!string.IsNullOrWhiteSpace(filter.DepartmentId))
                query["departmentId"] = filter.DepartmentId;

            if (!string.IsNullOrWhiteSpace(filter.UserTypeId))
                query["userTypeId"] = filter.UserTypeId;

            // paginação
            query["page"] = (filter.Page.HasValue && filter.Page.Value > 0 ? filter.Page.Value : 1).ToString();
            query["pageSize"] = (filter.PageSize.HasValue && filter.PageSize.Value > 0 ? filter.PageSize.Value : 10).ToString();

            // ordenação
            if (!string.IsNullOrWhiteSpace(filter.SortBy))
                query["sortBy"] = filter.SortBy!;
            if (filter.SortDesc.HasValue)
                query["sortDesc"] = filter.SortDesc.Value.ToString().ToLowerInvariant();

            var url = QueryHelpers.AddQueryString("api/User/search", query);

            var result = await _http.GetFromJsonAsync<PagedResult<UserModel>>(url);
            return result ?? new PagedResult<UserModel>
            {
                Items = Array.Empty<UserModel>(),
                Page = filter.Page ?? 1,
                PageSize = filter.PageSize ?? 10
            };
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
