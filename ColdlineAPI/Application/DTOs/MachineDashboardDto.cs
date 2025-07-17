namespace ColdlineAPI.Application.DTOs
{
    public class MachineDashboardDto
    {
        public int TotalMachines { get; set; }
        public int TotalProcesses { get; set; }
        public int FinishedMachines { get; set; }        
        public int TotalOccurrences { get; set; } 
        public List<MachineTotalProcessTimeDto> Machines { get; set; } = new();
        public List<MachineByTypeDto> MachineTypeCounts { get; set; } = new();
        public List<UserTotalProcessTimeDto> ProcessCountByUser { get; set; } = new();
        public List<ProcessTypeChartDto> ProcessCountByType { get; set; } = new();
        public List<MachineTypeAverageTimeDto> MachineTypeAverageTimes { get; set; } = new();


    }
}