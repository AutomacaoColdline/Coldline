using Microsoft.AspNetCore.Components;
using ColdlineWeb.Services;
using ColdlineWeb.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineWeb.Pages
{
    public class HomeComponent : ComponentBase
    {
        [Inject] public UserService UserService { get; set; } = default!;
        [Inject] public MachineService MachineService { get; set; } = default!;

        protected List<UserModel> users = new();
        protected List<MachineModel> machines = new();
        protected bool isLoading = true;
        protected string? errorMessage;

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
            finally
            {
                isLoading = false;
            }
        }

        protected async Task LoadMachines()
        {
            try
            {
                machines = await MachineService.GetAllMachinesAsync();
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
    }
}
