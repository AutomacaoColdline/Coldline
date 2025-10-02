using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.DTOs;
using ColdlineAPI.Application.Common;
using ColdlineAPI.Domain.Common;
using ColdlineAPI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using ColdlineAPI.Infrastructure.Utilities; 

namespace ColdlineAPI.Application.Interfaces
{
    public interface IProcessTypeService
    {
        Task<List<ProcessType>> GetAllProcessTypesAsync();
        Task<ProcessType?> GetProcessTypeByIdAsync(string id);
        Task<ProcessType> CreateProcessTypeAsync(ProcessType processType);
        Task<bool> UpdateProcessTypeAsync(string id, ProcessType processType);
        Task<bool> DeleteProcessTypeAsync(string id);
        Task<PagedResult<ProcessType>> SearchProcessTypesAsync(ProcessTypeFilter filter);
    }
}
