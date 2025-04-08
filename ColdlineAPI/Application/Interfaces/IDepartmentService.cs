using ColdlineAPI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Interfaces
{
    public interface IDepartmentService
    {
        Task<List<Department>> GetAllDepartmentsAsync();
        Task<Department?> GetDepartmentByIdAsync(string id);
        Task<Department> CreateDepartmentAsync(Department department);
        Task<(bool Success, string Message)> UpdateDepartmentAsync(string id, Department department);
        Task<(bool Success, string Message)> DeleteDepartmentAsync(string id);
    }
}
