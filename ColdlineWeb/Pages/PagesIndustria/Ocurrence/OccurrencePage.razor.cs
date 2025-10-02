using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;
using ColdlineWeb.Services;
using ColdlineWeb.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ColdlineWeb.Pages.PagesIndustria
{
    public class OccurrencePageBase : ComponentBase
    {
        [Inject] protected OccurrenceService OccurrenceService { get; set; } = default!;
        [Inject] protected OccurrenceTypeService OccurrenceTypeService { get; set; } = default!;
        [Inject] protected PartService PartService { get; set; } = default!;
        [Inject] protected UserService UserService { get; set; } = default!;
        [Inject] protected ProcessService ProcessService { get; set; } = default!;

        // ===== Dados para Tabela e Dropdowns =====
        protected List<OccurrenceModel> items = new();
        protected List<OccurrenceTypeModel> occurrenceTypes = new();
        protected List<PartModel> parts = new();
        protected List<UserModel> users = new();
        protected List<ProcessModel> processes = new();

        protected bool isLoading = true;
        protected string? errorMessage;

        // ===== Filtros =====
        protected OccurrenceSearchFilter filter = new();
        protected DateTime? startDate;
        protected DateTime? endDate;
        protected string? finishedStr;

        // ===== Modal =====
        protected bool showModal = false;
        protected bool isEditing = false;
        protected OccurrenceModel current = new();
        protected string? selectedOccurrenceTypeId;
        protected string? selectedPartId;
        protected string? selectedUserId;
        protected string? selectedProcessId;
        protected string? formError;

        // ===== Paginação =====
        protected int pageNumber = 1;
        protected int pageSize = 8;
        protected int totalPages = 1;
        protected string FixedDepartamentId = EnvironmentHelper.GetDepartmentId();
        protected List<OccurrenceModel> PagedItems => items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        // ===== Inicialização =====
        protected override async Task OnInitializedAsync()
        {
            await LoadDropdowns();
            await LoadAll();
        }

        // ===== Carregar Dropdowns =====
        protected async Task LoadDropdowns()
        {
            try
            {
                // Usuários
                var userFilter = new UserFilterModel
                {
                    DepartmentId = FixedDepartamentId,
                    Page = 1,
                    PageSize = 50
                };
                users = (await UserService.SearchUsersAsync(userFilter))?.Items?.ToList() ?? new List<UserModel>();

                // Processos
                var processFilter = new ProcessFilterModel
                {
                    DepartmentId = FixedDepartamentId,
                    Page = 1,
                    PageSize = 50
                };
                processes = (await ProcessService.SearchProcessesAsync(processFilter))?.Items?.ToList() ?? new List<ProcessModel>();

                // Tipos de Ocorrência
                var OTFilter = new OccurrenceTypeFilterModel
                {
                    DepartmentId = FixedDepartamentId
                };
                occurrenceTypes = (await OccurrenceTypeService.SearchAsync(OTFilter))?.Items?.ToList() ?? new List<OccurrenceTypeModel>();

                // Peças
                parts = await PartService.GetAllAsync() ?? new List<PartModel>();
            }
            catch (Exception ex)
            {
                errorMessage = "Erro ao carregar dropdowns (Usuários, Processos, Tipos, Peças).";
                Console.WriteLine(ex);
            }
        }

        // ===== Carregar Ocorrências =====
        protected async Task LoadAll()
        {
            try
            {
                isLoading = true;
                var oFilter = new OccurrenceSearchFilter
                {
                    DepartmentId = FixedDepartamentId
                };
                items = (await OccurrenceService.SearchAsync(oFilter))?.Items?.ToList() ?? new List<OccurrenceModel>();
                RecomputePages();
            }
            catch (Exception ex)
            {
                errorMessage = "Erro ao carregar ocorrências.";
                Console.WriteLine(ex);
            }
            finally
            {
                isLoading = false;
            }
        }

        // ===== Aplicar e Resetar Filtros =====
        protected async Task ApplyFilters()
        {
            try
            {
                filter.StartDate = startDate?.Date;
                filter.EndDate = endDate?.Date;
                filter.Finished = string.IsNullOrEmpty(finishedStr) ? null : finishedStr.Equals("true", StringComparison.OrdinalIgnoreCase);
                filter.DepartmentId = FixedDepartamentId;

                items = (await OccurrenceService.SearchAsync(filter))?.Items?.ToList() ?? new List<OccurrenceModel>();
                pageNumber = 1;
                RecomputePages();
            }
            catch (Exception ex)
            {
                errorMessage = "Erro ao aplicar filtros.";
                Console.WriteLine(ex);
            }
        }

        protected async Task ResetFilters()
        {
            filter = new OccurrenceSearchFilter { DepartmentId = FixedDepartamentId };
            startDate = endDate = null;
            finishedStr = null;
            await LoadAll();
        }

        private void RecomputePages()
        {
            totalPages = Math.Max(1, (int)Math.Ceiling((double)items.Count / pageSize));
            if (pageNumber > totalPages) pageNumber = totalPages;
        }

        // ===== Modal Add / Edit =====
        protected void OpenAddModal()
        {
            formError = null;
            isEditing = false;
            current = new OccurrenceModel
            {
                Process = new ReferenceEntity(),
                User = new ReferenceEntity(),
                OccurrenceType = new OccurrenceTypeModel(),
                Part = new ReferenceEntity(),
                StartDate = DateTime.Now,
                EndDate = DateTime.Now
            };
            selectedUserId = selectedProcessId = selectedOccurrenceTypeId = selectedPartId = string.Empty;
            showModal = true;
        }

        protected void OpenEditModal(OccurrenceModel o)
        {
            formError = null;
            isEditing = true;

            current = new OccurrenceModel
            {
                Id = o.Id,
                CodeOccurrence = o.CodeOccurrence,
                ProcessTime = o.ProcessTime,
                StartDate = o.StartDate,
                EndDate = o.EndDate,
                Process = new ReferenceEntity { Id = o.Process?.Id ?? "", Name = o.Process?.Name ?? "" },
                User = new ReferenceEntity { Id = o.User?.Id ?? "", Name = o.User?.Name ?? "" },
                Description = o.Description,
                Finished = o.Finished,
                OccurrenceType = o.OccurrenceType ?? new OccurrenceTypeModel(),
                Part = o.Part ?? new ReferenceEntity()
            };

            selectedUserId = current.User?.Id;
            selectedProcessId = current.Process?.Id;
            selectedOccurrenceTypeId = current.OccurrenceType?.Id;
            selectedPartId = current.Part?.Id;

            showModal = true;
        }

        protected void CloseModal() => showModal = false;

        // ===== Dropdown Changes =====
        protected void OnUserChanged()
        {
            var user = users.FirstOrDefault(u => u.Id == selectedUserId);
            current.User = user != null ? new ReferenceEntity { Id = user.Id, Name = user.Name } : new ReferenceEntity();
        }

        protected void OnProcessChanged()
        {
            var process = processes.FirstOrDefault(p => p.Id == selectedProcessId);
            current.Process = process != null ? new ReferenceEntity { Id = process.Id, Name = process.IdentificationNumber } : new ReferenceEntity();
        }

        protected void OnTypeChanged()
        {
            var type = occurrenceTypes.FirstOrDefault(t => t.Id == selectedOccurrenceTypeId);
            current.OccurrenceType = type ?? new OccurrenceTypeModel();
        }

        protected void OnPartChanged()
        {
            var part = parts.FirstOrDefault(p => p.Id == selectedPartId);
            current.Part = part != null ? new ReferenceEntity { Id = part.Id, Name = part.Name } : new ReferenceEntity();
        }

        // ===== Salvar =====
        protected async Task Save()
        {
            formError = null;

            if (string.IsNullOrWhiteSpace(current.OccurrenceType?.Id))
            {
                formError = "Selecione o Tipo de Ocorrência.";
                return;
            }
            if (string.IsNullOrWhiteSpace(current.User?.Id))
            {
                formError = "Selecione o Usuário.";
                return;
            }
            if (string.IsNullOrWhiteSpace(current.Process?.Id))
            {
                formError = "Selecione o Processo.";
                return;
            }

            try
            {
                if (isEditing)
                {
                    var ok = await OccurrenceService.UpdateAsync(current.Id, current);
                    if (!ok) throw new InvalidOperationException("Falha ao atualizar ocorrência.");
                }
                else
                {
                    var created = await OccurrenceService.CreateAsync(current);
                    if (created == null) throw new InvalidOperationException("Falha ao criar ocorrência.");
                }

                showModal = false;
                await LoadAll();
            }
            catch (Exception ex)
            {
                formError = "Erro ao salvar ocorrência.";
                Console.WriteLine(ex);
            }
        }

        // ===== Ações Tabela =====
        protected async Task Delete(string id)
        {
            try
            {
                var ok = await OccurrenceService.DeleteAsync(id);
                if (!ok) throw new InvalidOperationException("Falha ao excluir ocorrência.");
                await LoadAll();
            }
            catch (Exception ex)
            {
                errorMessage = "Erro ao excluir ocorrência.";
                Console.WriteLine(ex);
            }
        }

        protected async Task End(string id)
        {
            try
            {
                var ok = await OccurrenceService.FinalizeOccurrenceAsync(id);
                if (!ok) throw new InvalidOperationException("Falha ao finalizar ocorrência.");
                await LoadAll();
            }
            catch (Exception ex)
            {
                errorMessage = "Erro ao finalizar ocorrência.";
                Console.WriteLine(ex);
            }
        }

        // ===== Paginação =====
        protected async Task GoNext()
        {
            if (pageNumber < totalPages)
            {
                pageNumber++;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected async Task GoPrev()
        {
            if (pageNumber > 1)
            {
                pageNumber--;
                await InvokeAsync(StateHasChanged);
            }
        }
    }
}
