using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.DTOs;
using ColdlineAPI.Application.Common;
using ColdlineAPI.Domain.Common;
using ColdlineAPI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System; 
using ColdlineAPI.Infrastructure.Utilities; 

namespace ColdlineAPI.Application.Interfaces
{
    public interface IProcessService
    {
        Task<List<Process>> GetAllProcessesAsync();
        Task<Process?> GetProcessByIdAsync(string id);
        Task<Process> CreateProcessAsync(Process process);
        Task<bool> UpdateProcessAsync(string id, Process process);
        Task<bool> DeleteProcessAsync(string id);
        Task<PagedResult<Process>> SearchProcessAsync(ProcessFilter filter);
        Task<Process?> StartProcessAsync(string identificationNumber, string processTypeId, string? machineId, bool preIndustrialization, bool reWork, bool prototype);
        Task<bool> UpdateProcessTimeInDatabase(string processId, string processTime);
        Task<bool> EndProcessAsync(string processId, bool Finished, StartOccurrenceRequest requestOccurrence);
        Task<ProcessStatisticsDto> GetProcessStatisticsAsync(string processId, string processTypeId);
        Task<UserProcessDetailsDto> GetUserProcessDataAsync(string userId);
        Task<List<ProcessByDateDto>> GetProcessCountByStartDateAsync(DateTime start, DateTime end);
        Task<List<ProcessTypeChartDto>> GetProcessCountByTypeAndDateAsync(DateTime start, DateTime end);
        Task<List<ProcessUserChartDto>> GetProcessCountByUserAsync(DateTime start, DateTime end);
        Task<List<UserTotalProcessTimeDto>> GetTotalProcessTimeByUserAsync(DateTime start, DateTime end);
        Task<List<ProcessTypeTotalTimeDto>> GetTotalProcessTimeByProcessTypeAsync(DateTime start, DateTime end);
        Task<List<IndividualUserProcessDto>> GetIndividualProcessTimesByUserAsync(string userId, DateTime startDate, DateTime endDate, bool? preIndustrialization = null);
        Task<byte[]> GenerateExcelReportAsync(DateTime startDate, DateTime endDate);
        Task<List<MonthlyWorkSummaryDto>> GetUserMonthlyWorkSummaryAsync(string userId, int year, int month);

    }
}
