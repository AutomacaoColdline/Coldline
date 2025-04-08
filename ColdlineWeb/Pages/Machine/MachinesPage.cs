using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using ColdlineWeb.Util;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Enum;
using ColdlineWeb.Models.Filter;
using ColdlineWeb.Services;

namespace ColdlineWeb.Pages
{
    public class MachinesPage : ComponentBase
    {
        [Inject] protected HttpClient Http { get; set; } = default!;
        [Inject] protected MachineService machineService { get; set; } = default!;

        protected List<MachineModel> machines = new();
        protected MachineModel currentMachine = new();

        protected List<ReferenceEntity> machineTypes = new();
        protected List<ProcessModel> processes = new();

        protected bool isLoading = true;
        protected bool showModal = false;
        protected bool isEditing = false;
        protected string? errorMessage;

        protected MachineFilterModel filter = new();

        protected int pageNumber = 1;
        protected int totalPages = 1;
        protected const int defaultPageSize = 10;

        protected bool CanGoNext => pageNumber < totalPages;
        protected bool CanGoPrevious => pageNumber > 1;

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
                processes = await Http.GetFromJsonAsync<List<ProcessModel>>("api/Process") ?? new();
                machineTypes = await Http.GetFromJsonAsync<List<ReferenceEntity>>("api/MachineType") ?? new();
                await ApplyFilters();
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

        protected async Task ApplyFilters()
        {
            try
            {
                filter.Page = pageNumber;
                filter.PageSize = defaultPageSize;

                var result = await machineService.SearchMachinesAsync(filter);
                machines = result;
                totalPages = result.Count == defaultPageSize ? pageNumber + 1 : pageNumber;
            }
            catch (Exception ex)
            {
                errorMessage = "Erro ao aplicar filtros.";
                Console.WriteLine($"[ERRO] {ex.Message}");
            }
        }

        protected async Task ResetFilters()
        {
            filter = new MachineFilterModel();
            pageNumber = 1;
            await ApplyFilters();
        }

        protected void CloseModal() => showModal = false;

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
                currentMachine.Status = MachineStatus.WaitingProduction;

                if (isEditing)
                    await Http.PutAsJsonAsync($"api/Machine/{currentMachine.Id}", currentMachine);
                else
                    await Http.PostAsJsonAsync("api/Machine", currentMachine);

                await ApplyFilters();
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
                await ApplyFilters();
            }
            catch (Exception ex)
            {
                errorMessage = "Erro ao excluir máquina.";
                Console.WriteLine($"[ERRO] {ex.Message}");
            }
        }

        protected async Task GoToNextPage()
        {
            if (CanGoNext)
            {
                pageNumber++;
                await ApplyFilters();
            }
        }

        protected async Task GoToPreviousPage()
        {
            if (CanGoPrevious)
            {
                pageNumber--;
                await ApplyFilters();
            }
        }

        protected void OpenMachineTypeModal()
        {
            newMachineType = new MachineTypeModel();
            showMachineTypeModal = true;
            isEditingMachineType = false;
        }

        protected void CloseMachineTypeModal() => showMachineTypeModal = false;

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
