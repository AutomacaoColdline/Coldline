using System.Net.Http.Json;
using ColdlineWeb.Models;

namespace ColdlineWeb.Services;

public class UserService
{
    private readonly HttpClient _httpClient;

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<User>> GetUsersAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<User>>("http://10.0.0.44:5000/api/User") ?? new List<User>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao buscar usuários: {ex.Message}");
            return new List<User>(); // Retorna lista vazia em caso de erro
        }
    }
}
