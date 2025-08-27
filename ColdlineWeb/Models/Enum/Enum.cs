using System.ComponentModel.DataAnnotations;

namespace ColdlineWeb.Models.Enum
{
    public enum MachineStatus
    {
        [Display(Name = "Aguardando Produção")]
        WaitingProduction = 1,

        [Display(Name = "Em Progresso")]
        InProgress = 2,

        [Display(Name = "Em Ocorrência")]
        InOcurrence = 3,

        [Display(Name = "Retrabalho")]
        InRework = 4,

        [Display(Name = "Finalizado")]
        Finished = 5,
        [Display(Name = "Pausada")]
        Stop = 6
    }
}
