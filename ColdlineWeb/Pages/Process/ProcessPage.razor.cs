using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Net.Http.Json;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;
using ColdlineWeb.Services;

namespace ColdlineWeb.Pages.Process
{
    public class ProcessPageBase : ComponentBase
    {
        [Inject] protected ProcessService ProcessService { get; set; } = default!;
        [Inject] protected HttpClient Http { get; set; } = default!;

        // Lista de processos
        protected List<ProcessModel> ProcessList = new();

        // Dropdowns de referências
        protected List<ReferenceEntity> UsersRef = new();
        protected List<ReferenceEntity> Departments = new();
        protected List<ReferenceEntity> ProcessTypes = new();
        protected List<ReferenceEntity> Machines = new();
        protected List<ReferenceEntity> Occurrences = new();
        protected List<string> SelectedOccurrences = new();

        // Processo atual para Add/Edit
        protected ProcessModel CurrentProcess = new();

        // Controle do modal
        protected bool ShowModal = false;
        protected bool IsEditing = false;
        protected string? ErrorMessage;
        protected bool IsLoading = true;

        protected override async Task OnInitializedAsync()
        {
            await LoadDropdownData();
            await LoadProcesses();
        }

        protected async Task LoadDropdownData()
        {
            try
            {
                UsersRef = await Http.GetFromJsonAsync<List<ReferenceEntity>>("api/User") ?? new();
                // Supondo que sua API de Departamento retorna DepartmentModel e você converte para ReferenceEntity:
                var deptModels = await Http.GetFromJsonAsync<List<DepartmentModel>>("api/Department") ?? new List<DepartmentModel>();
                Departments = deptModels.ConvertAll(d => new ReferenceEntity { Id = d.Id, Name = d.Name });
                ProcessTypes = await Http.GetFromJsonAsync<List<ReferenceEntity>>("api/ProcessType") ?? new();
                Machines = await Http.GetFromJsonAsync<List<ReferenceEntity>>("api/Machine") ?? new();
                Occurrences = await Http.GetFromJsonAsync<List<ReferenceEntity>>("api/Occurrence") ?? new();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Erro ao carregar dados para dropdowns.";
                Console.WriteLine(ex.Message);
            }
        }

        protected async Task LoadProcesses()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                ProcessList = await ProcessService.GetAllProcessesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Erro ao carregar processos.";
                Console.WriteLine(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Métodos CRUD de processo
        protected void OpenAddProcessModal()
        {
            CurrentProcess = new ProcessModel();
            ShowModal = true;
            IsEditing = false;
        }

        protected void OpenEditProcessModal(ProcessModel process)
        {
            CurrentProcess = process;
            ShowModal = true;
            IsEditing = true;
        }

        protected void CloseProcessModal()
        {
            ShowModal = false;
        }

        protected async Task SaveProcess()
        {
            // Preenche os nomes das referências com base nos IDs selecionados
            CurrentProcess.User.Name = UsersRef.FirstOrDefault(u => u.Id == CurrentProcess.User.Id)?.Name ?? "Desconhecido";
            CurrentProcess.Department.Name = Departments.FirstOrDefault(d => d.Id == CurrentProcess.Department.Id)?.Name ?? "Desconhecido";
            CurrentProcess.ProcessType.Name = ProcessTypes.FirstOrDefault(pt => pt.Id == CurrentProcess.ProcessType.Id)?.Name ?? "Desconhecido";
            var selectedMachine = Machines.FirstOrDefault(m => m.Id == CurrentProcess.Machine.Id);
            if (selectedMachine != null)
            {
                CurrentProcess.Machine.Name = selectedMachine.Name;
            }
            else
            {
                ErrorMessage = "Máquina não encontrada!";
                return;
            }
            // Preenche as ocorrências
            CurrentProcess.Occurrences = Occurrences
                .Where(o => SelectedOccurrences.Contains(o.Id))
                .Select(o => new ReferenceEntity { Id = o.Id, Name = o.Name })
                .ToList();

            HttpResponseMessage response;
            if (IsEditing)
                response = await Http.PutAsJsonAsync($"api/Process/{CurrentProcess.Id}", CurrentProcess);
            else
                response = await Http.PostAsJsonAsync("api/Process", CurrentProcess);

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = $"Erro ao salvar processo. Status: {response.StatusCode}";
                Console.WriteLine(await response.Content.ReadAsStringAsync());
                return;
            }

            CloseProcessModal();
            await LoadProcesses();
        }

        protected async Task DeleteProcess(string id)
        {
            try
            {
                await Http.DeleteAsync($"api/Process/{id}");
                await LoadProcesses();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Erro ao excluir o processo.";
                Console.WriteLine(ex.Message);
            }
        }

        protected void HandleOccurrenceSelection(ChangeEventArgs e)
        {
            if (e.Value is IEnumerable<string> values)
                SelectedOccurrences = values.ToList();
            else if (e.Value is string singleValue)
                SelectedOccurrences = new List<string> { singleValue };
        }
    }
}
