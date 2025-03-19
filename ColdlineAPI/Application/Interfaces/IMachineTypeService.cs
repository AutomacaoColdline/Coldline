using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Application.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Interfaces
{
    public interface IMachineTypeService
    {
        Task<List<MachineType>> GetAllMachineTypesAsync();
        Task<MachineType?> GetMachineTypeByIdAsync(string id);
        Task<MachineType> CreateMachineTypeAsync(MachineType machine);
       Task<bool> UpdateMachineTypeAsync(string id, MachineType machine);
        Task<bool> DeleteMachineTypeAsync(string id);
    }
}
