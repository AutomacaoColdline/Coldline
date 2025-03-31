using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Enum;
using ColdlineWeb.Models.Filter;

namespace ColdlineWeb.Pages
{
    public class MachinesPage : ComponentBase
    {
        [Inject] protected HttpClient Http { get; set; } = default!;

        // Lista principal
        protected List<MachineModel> machines = new();
        protected MachineModel currentMachine = new();

        // Relacionamentos
        protected List<ReferenceEntity> machineTypes = new();
        protected List<ProcessModel> processes = new();

        // Estado da UI
        protected bool isLoading = true;
        protected bool showModal = false;
        protected bool isEditing = false;
        protected string? errorMessage;

        // Filtros
        protected MachineFilterModel filter = new();

        // Modal de MachineType
        protected bool showMachineTypeModal = false;
        protected bool isEditingMachineType = false;
        protected MachineTypeModel newMachineType = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        protected async Task LoadData()
        {
            try
            {
                isLoading = true;
                machines = await Http.GetFromJsonAsync<List<MachineModel>>("api/Machine") ?? new();
                processes = await Http.GetFromJsonAsync<List<ProcessModel>>("api/Process") ?? new();
                machineTypes = await Http.GetFromJsonAsync<List<ReferenceEntity>>("api/MachineType") ?? new();
            }
            catch (Exception ex)
            {
                errorMessage = "Erro ao carregar dados.";
                Console.WriteLine($"[ERRO] {ex.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }

        protected void OpenAddMachineModal()
        {
            currentMachine = new MachineModel
            {
                MachineType = new ReferenceEntity()
            };
            isEditing = false;
            showModal = true;
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
                MachineType = machine.MachineType ?? new ReferenceEntity(),
                Process = machine.Process,
                Quality = machine.Quality,
                Monitoring = machine.Monitoring,
                Time = machine.Time,
                Status = machine.Status
            };
            isEditing = true;
            showModal = true;
        }

        protected async Task SaveMachine()
        {
            try
            {
                var fetchedType = await Http.GetFromJsonAsync<ReferenceEntity>($"api/MachineType/{currentMachine.MachineType.Id}");
                currentMachine.MachineType.Name = fetchedType?.Name ?? "";

                currentMachine.Process = null;
                currentMachine.Monitoring = null;
                currentMachine.Quality = null;

                if (isEditing)
                    await Http.PutAsJsonAsync($"api/Machine/{currentMachine.Id}", currentMachine);
                else
                    await Http.PostAsJsonAsync("api/Machine", currentMachine);

                await LoadData();
                showModal = false;
            }
            catch (Exception ex)
            {
                errorMessage = "Erro ao salvar máquina.";
                Console.WriteLine($"[ERRO] {ex.Message}");
            }
        }

        protected async Task DeleteMachine(string id)
        {
            try
            {
                await Http.DeleteAsync($"api/Machine/{id}");
                await LoadData();
            }
            catch (Exception ex)
            {
                errorMessage = "Erro ao excluir máquina.";
                Console.WriteLine($"[ERRO] {ex.Message}");
            }
        }

        protected async Task ApplyFilters()
        {
            try
            {
                var response = await Http.PostAsJsonAsync("api/Machine/search", filter);
                if (response.IsSuccessStatusCode)
                    machines = await response.Content.ReadFromJsonAsync<List<MachineModel>>() ?? new();
                else
                    errorMessage = "Erro ao aplicar os filtros.";
            }
            catch (Exception ex)
            {
                errorMessage = "Erro inesperado ao aplicar filtros.";
                Console.WriteLine($"[ERRO] {ex.Message}");
            }
        }

        protected async Task ResetFilters()
        {
            filter = new MachineFilterModel();
            await ApplyFilters();
        }

        protected void CloseModal()
        {
            showModal = false;
        }

        // ========== MODAL DE MACHINE TYPE ==========

        protected void OpenMachineTypeModal()
        {
            newMachineType = new MachineTypeModel();
            showMachineTypeModal = true;
            isEditingMachineType = false;
        }

        protected void CloseMachineTypeModal()
        {
            showMachineTypeModal = false;
        }

        protected async Task SaveMachineType()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newMachineType.Id))
                    await Http.PostAsJsonAsync("api/MachineType", newMachineType);
                else
                    await Http.PutAsJsonAsync($"api/MachineType/{newMachineType.Id}", newMachineType);

                await ReloadMachineTypes();
                showMachineTypeModal = false;
            }
            catch (Exception ex)
            {
                errorMessage = "Erro ao salvar tipo de máquina.";
                Console.WriteLine($"[ERRO] {ex.Message}");
            }
        }

        protected async Task ReloadMachineTypes()
        {
            machineTypes = await Http.GetFromJsonAsync<List<ReferenceEntity>>("api/MachineType") ?? new();
        }
    }
}
