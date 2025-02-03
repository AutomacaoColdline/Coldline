using ColdlineAPI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Interfaces
{
    public interface IUserTypeService
    {
        Task<List<UserType>> GetAllUserTypesAsync();
        Task<UserType?> GetUserTypeByIdAsync(string id);
        Task<UserType> CreateUserTypeAsync(UserType userType);
        Task<bool> UpdateUserTypeAsync(string id, UserType userType);
        Task<bool> DeleteUserTypeAsync(string id);
    }
}
