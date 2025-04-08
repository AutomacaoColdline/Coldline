using ColdlineAPI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Application.Interfaces
{
    public interface IMachineTypeService
    {
        Task<List<MachineType>> GetAllMachineTypesAsync();
        Task<MachineType?> GetMachineTypeByIdAsync(string id);
        Task<MachineType> CreateMachineTypeAsync(MachineType machineType);
        Task<bool> UpdateMachineTypeAsync(string id, MachineType machineType);
        Task<bool> DeleteMachineTypeAsync(string id);
    }
}
