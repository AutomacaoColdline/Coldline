using Microsoft.AspNetCore.Components;
using ColdlineWeb.Services;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Enum;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ColdlineWeb.Pages
{
    public partial class HomePage : ComponentBase
    {
        [Inject] public UserService UserService { get; set; } = default!;
        [Inject] public MachineService MachineService { get; set; } = default!;

        protected List<UserModel> users = new();
        protected List<MachineModel> machines = new();
        protected int totalMachines;
        protected int machinesWaitingProduction;
        protected int machinesInProgress;
        protected int machinesInOccurrence;
        protected int machinesFinished;
        protected bool isLoading = true;
        protected string? errorMessage;

        private int currentPage = 0;
        private const int pageSize = 8; // Exibir 8 usuários por vez

        protected List<UserModel> PaginatedUsers => users.Skip(currentPage * pageSize).Take(pageSize).ToList();

        protected bool isFirstPage => currentPage == 0;
        protected bool isLastPage => (currentPage + 1) * pageSize >= users.Count;

        protected override async Task OnInitializedAsync()
        {
            await LoadUsers();
            await LoadMachines();
        }

        protected async Task LoadUsers()
        {
            try
            {
                users = await UserService.GetUsersAsync();
            }
            catch
            {
                errorMessage = "Erro ao carregar usuários.";
            }
        }

        protected async Task LoadMachines()
        {
            try
            {
                machines = await MachineService.GetAllMachinesAsync();
                totalMachines = machines.Count;
                machinesWaitingProduction = machines.Count(m => m.Status == MachineStatus.WaitingProduction);
                machinesInProgress = machines.Count(m => m.Status == MachineStatus.InProgress);
                machinesInOccurrence = machines.Count(m => m.Status == MachineStatus.InOcurrence);
                machinesFinished = machines.Count(m => m.Status == MachineStatus.Finished);
            }
            catch
            {
                errorMessage = "Erro ao carregar máquinas.";
            }
            finally
            {
                isLoading = false;
            }
        }

        protected void NextPage()
        {
            if (!isLastPage)
            {
                currentPage++;
            }
        }

        protected void PreviousPage()
        {
            if (!isFirstPage)
            {
                currentPage--;
            }
        }
    }
}
