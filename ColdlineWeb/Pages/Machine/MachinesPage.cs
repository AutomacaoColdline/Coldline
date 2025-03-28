using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Enum;

namespace ColdlineWeb.Pages
{
    public class MachinesPage : ComponentBase
    {
        [Inject] protected HttpClient Http { get; set; } = default!;

        protected List<MachineModel> machines = new();
        protected List<ProcessModel> processes = new();
        protected List<ReferenceEntity> machineTypes = new();
        protected MachineModel currentMachine = new();
        protected bool isLoading = true;
        protected bool showModal = false;
        protected bool isEditing = false;
        protected string? errorMessage;

        protected override async Task OnInitializedAsync() => await LoadData();

        protected async Task LoadData()
        {
            try
            {
                machines = await Http.GetFromJsonAsync<List<MachineModel>>("api/Machine") ?? new();
                processes = await Http.GetFromJsonAsync<List<ProcessModel>>("api/Process") ?? new();
                machineTypes = await Http.GetFromJsonAsync<List<ReferenceEntity>>("api/MachineType") ?? new();
            }
            catch (Exception ex)
            {
                errorMessage = "Erro ao carregar dados.";
                Console.WriteLine(ex);
            }
            finally
            {
                isLoading = false;
            }
        }

        protected void OpenAddMachineModal()
        {
            currentMachine = new MachineModel { MachineType = new ReferenceEntity() };
            showModal = true;
            isEditing = false;
        }

        protected void OpenEditMachineModal(MachineModel machine)
        {
            currentMachine = new MachineModel
            {
                Id = machine.Id,
                CustomerName = machine.CustomerName,
                IdentificationNumber = machine.IdentificationNumber,
                Phase = machine.Phase,
                Voltage = machine.Voltage,
                MachineType = machine.MachineType ?? new ReferenceEntity()
            };
            showModal = true;
            isEditing = true;
        }

        protected async Task SaveMachine()
        {
            var fetchedMachineType = await Http.GetFromJsonAsync<ReferenceEntity>($"api/MachineType/{currentMachine.MachineType.Id}");
            currentMachine.MachineType.Name = fetchedMachineType.Name;

            currentMachine.Process = null;
            currentMachine.Monitoring = null;
            currentMachine.Quality = null;

            if (isEditing)
                await Http.PutAsJsonAsync($"api/Machine/{currentMachine.Id}", currentMachine);
            else
                await Http.PostAsJsonAsync("api/Machine", currentMachine);

            showModal = false;
            await LoadData();
        }

        protected async Task DeleteMachine(string id)
        {
            await Http.DeleteAsync($"api/Machine/{id}");
            await LoadData();
        }

        protected void CloseModal() => showModal = false;
    }
}
