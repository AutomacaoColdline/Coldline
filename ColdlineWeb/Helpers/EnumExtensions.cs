using ColdlineWeb.Models.Enum;

namespace ColdlineWeb.Helpers
{
    public static class EnumExtensions
    {
        public static string ToPortugueseString(this MachineStatus status)
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
