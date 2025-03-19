using ColdlineWeb.Helpers;

namespace ColdlineWeb.Models.Enum
{
    public enum MachineStatus
    {
        WaitingProduction = 1,     
        InProgress = 2, 
        InOcurrence = 3,     
        InRework = 4,    
        Finished = 5,      
    }
}
