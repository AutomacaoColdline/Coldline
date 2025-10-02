using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Enum;
using ColdlineWeb.Models.Filter;
using ColdlineWeb.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using ColdlineWeb.Util;

namespace ColdlineWeb.Pages.PagesIndustria.Machine
{
    public class MachinesPage : ComponentBase
    {
        [Inject] protected HttpClient HttpClient { get; set; } = default!;
        [Inject] protected MachineService MachineService { get; set; } = default!;

        // Listas e objetos de dados
        protected List<MachineModel> machines { get; set; } = new();
        protected MachineModel currentMachine { get; set; } = new();
        protected List<ReferenceEntity> machineTypes { get; set; } = new();
        protected List<ProcessModel> processes { get; set; } = new();

        // Controle de UI
        protected bool isLoading { get; set; } = true;
        protected bool showModal { get; set; } = false;
        protected bool isEditing { get; set; } = false;
        protected string? errorMessage { get; set; }

        // Filtros
        protected MachineFilterModel filter { get; set; } = new();

        // Paginação
        protected int pageNumber { get; set; } = 1;
        protected int totalPages { get; set; } = 1;
        protected const int DefaultPageSize = 8;

        protected bool CanGoNext => pageNumber < totalPages;
        protected bool CanGoPrevious => pageNumber > 1;

        // Modal de tipos de máquina
        protected bool showMachineTypeModal { get; set; } = false;
        protected bool isEditingMachineType { get; set; } = false;
        protected MachineTypeModel newMachineType { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();
        }

        // Carregar dados iniciais
        protected async Task LoadDataAsync()
        {
            try
            {
                isLoading = true;

                processes = await HttpClient.GetFromJsonAsync<List<ProcessModel>>("api/Process") ?? new List<ProcessModel>();
                machineTypes = await HttpClient.GetFromJsonAsync<List<ReferenceEntity>>("api/MachineType") ?? new List<ReferenceEntity>();

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

        // Aplicar filtros e atualizar lista de máquinas
        protected async Task ApplyFilters()
        {
            try
            {
                filter.Page = pageNumber;
                filter.PageSize = DefaultPageSize;

                var pagedResult = await MachineService.SearchMachinesAsync(filter);

                machines = pagedResult?.Items != null ? new List<MachineModel>(pagedResult.Items) : new List<MachineModel>();
                totalPages = pagedResult != null && pagedResult.Total > 0
                    ? (int)Math.Ceiling((double)pagedResult.Total / DefaultPageSize)
                    : machines.Count == DefaultPageSize ? pageNumber + 1 : pageNumber;
            }
            catch (Exception ex)
            {
                errorMessage = "Erro ao aplicar filtros.";
                Console.WriteLine($"[ERRO] {ex.Message}");
            }
        }

        // Resetar filtros
        protected async Task ResetFilters()
        {
            filter = new MachineFilterModel();
            pageNumber = 1;
            await ApplyFilters();
        }

        // Finalizar máquina
        protected async Task FinalizeMachineAsync(string machineId)
        {
            try
            {
                var success = await MachineService.FinalizeMachineAsync(machineId);
                if (success) await ApplyFilters();
                else errorMessage = "Falha ao finalizar máquina.";
            }
            catch (Exception ex)
            {
                errorMessage = "Erro ao finalizar máquina.";
                Console.WriteLine($"[ERRO] {ex.Message}");
            }
        }

        // Modais
        protected void CloseModal() => showModal = false;

        protected void OpenAddMachineModal()
        {
            currentMachine = new MachineModel { MachineType = new ReferenceEntity() };
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

        // Salvar máquina
        protected async Task SaveMachine()
        {
            try
            {
                if (!string.IsNullOrEmpty(currentMachine.MachineType?.Id))
                {
                    var fetchedType = await HttpClient.GetFromJsonAsync<ReferenceEntity>($"api/MachineType/{currentMachine.MachineType.Id}");
                    currentMachine.MachineType.Name = fetchedType?.Name ?? "";
                }

                currentMachine.Process = null;
                currentMachine.Monitoring = null;
                currentMachine.Quality = null;
                currentMachine.Status = MachineStatus.WaitingProduction;

                if (isEditing)
                    await HttpClient.PutAsJsonAsync($"api/Machine/{currentMachine.Id}", currentMachine);
                else
                    await HttpClient.PostAsJsonAsync("api/Machine", currentMachine);

                await ApplyFilters();
                showModal = false;
            }
            catch (Exception ex)
            {
                errorMessage = "Erro ao salvar máquina.";
                Console.WriteLine($"[ERRO] {ex.Message}");
            }
        }

        // Excluir máquina
        protected async Task DeleteMachine(string id)
        {
            try
            {
                await HttpClient.DeleteAsync($"api/Machine/{id}");
                await ApplyFilters();
            }
            catch (Exception ex)
            {
                errorMessage = "Erro ao excluir máquina.";
                Console.WriteLine($"[ERRO] {ex.Message}");
            }
        }

        // Paginação
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

        // Modal de tipos de máquina
        protected void OpenMachineTypeModal()
        {
            newMachineType = new MachineTypeModel();
            showMachineTypeModal = true;
            isEditingMachineType = false;
        }

        protected void CloseMachineTypeModal() => showMachineTypeModal = false;

        protected async Task SaveMachineTypeAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newMachineType.Id))
                    await HttpClient.PostAsJsonAsync("api/MachineType", newMachineType);
                else
                    await HttpClient.PutAsJsonAsync($"api/MachineType/{newMachineType.Id}", newMachineType);

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
            machineTypes = await HttpClient.GetFromJsonAsync<List<ReferenceEntity>>("api/MachineType") ?? new List<ReferenceEntity>();
        }
    }
}
