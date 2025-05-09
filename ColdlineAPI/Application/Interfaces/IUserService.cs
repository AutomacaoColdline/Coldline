using System.Collections.Generic;
using System.Threading.Tasks;
using ColdlineAPI.Domain.Entities;

namespace ColdlineAPI.Application.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetUsersAsync();
        Task<User?> GetUserByIdAsync(string id);
        Task<User> CreateUserAsync(User user);
        Task<(bool Success, string Message)> UpdateUserAsync(string id, User user);
        Task<bool> DeleteUserAsync(string id);
        Task<(List<User> Items, long TotalCount)> SearchUsersAsync(string? name,string? email,string? departmentId,string? userTypeId,int pageNumber,int pageSize);
        Task<User?> AuthenticateUserAsync(string email, string password);
        string GenerateJwtToken(User user); 
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword);
        Task<User?> GetUserByIdentificationNumberAsync(string identificationNumber);
        Task<byte[]> GenerateExcelWithSampleDataAsync();
        Task<byte[]> GeneratePdfWithAgeChartAsync();

    }
}
