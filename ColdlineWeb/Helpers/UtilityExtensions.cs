using ColdlineWeb.Models.Enum;

namespace ColdlineWeb.Helpers
{
    public static class UtilityExtensions
    {
        public static string GetTranslatedStatus(this MachineStatus status)
        {
            return status switch
            {
                MachineStatus.WaitingProduction => "Aguardando Produção",
                MachineStatus.InProgress => "Em Progresso",
                MachineStatus.InOcurrence => "Em Ocorrência",
                MachineStatus.InRework => "Retrabalho",
                MachineStatus.Finished => "Finalizado",
                _ => "Desconhecido"
            };
        }
    }
}
