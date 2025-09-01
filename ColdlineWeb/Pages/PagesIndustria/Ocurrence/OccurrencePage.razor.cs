using Microsoft.AspNetCore.Components;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;
using ColdlineWeb.Services;

namespace ColdlineWeb.Pages.PagesIndustria
{
    public class OccurrencePageBase : ComponentBase
    {
        [Inject] protected OccurrenceService OccurrenceService { get; set; } = default!;
        [Inject] protected OccurrenceTypeService OccurrenceTypeService { get; set; } = default!;
        [Inject] protected PartService PartService { get; set; } = default!;
        [Inject] protected UserService UserService { get; set; } = default!;

        protected List<OccurrenceModel> items = new();
        protected List<OccurrenceTypeModel> occurrenceTypes = new();
        protected List<PartModel> parts = new();
        protected List<UserModel> users = new();

        protected bool isLoading = true;
        protected string? errorMessage;

        // filtros de busca
        protected OccurrenceSearchFilter filter = new();
        protected DateTime? startDate;   // datas do <InputDate>
        protected DateTime? endDate;
        protected string? finishedStr;   // "", "true", "false"

        // modal
        protected bool showModal = false;
        protected bool isEditing = false;
        protected OccurrenceModel current = new();
        protected string? selectedOccurrenceTypeId;
        protected string? selectedPartId;
        protected string? formError;

        // paginação (client-side)
        protected int pageNumber = 1;
        protected int pageSize = 8;
        protected int totalPages = 1;

        protected string FixedDepartamentId = "67f41c323a596bf4e95bfe6d";

        protected List<OccurrenceModel> PagedItems =>
            items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        protected override async Task OnInitializedAsync()
        {
            await LoadAll();
        }

        protected async Task LoadAll()
        {
            try
            {
                isLoading = true;
                errorMessage = null;
                var OTFilter = new OccurrenceTypeFilterModel
                {
                    DepartmentId = FixedDepartamentId,
                };
                occurrenceTypes = await OccurrenceTypeService.SearchAsync(OTFilter);
                //occurrenceTypes = await OccurrenceTypeService.GetAllAsync();
               
                var usersPage = await UserService.SearchUsersAsync(
                    name: null,
                    email: null,
                    departmentId: FixedDepartamentId,
                    userTypeId: null,
                    pageNumber: 1,
                    pageSize: 5
                );
                users = usersPage.Items ?? new List<UserModel>();

                //users = await UserService.GetUsersAsync();
                parts = await PartService.GetAllAsync();

                var OFilter = new OccurrenceSearchFilter
                {
                    DepartmentId = FixedDepartamentId,
                };

                items = await OccurrenceService.SearchAsync(OFilter);

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

        // ====== BUSCA ======
        protected async Task ApplyFilters()
        {
            try
            {
                filter.StartDate = startDate?.Date;
                filter.EndDate   = endDate?.Date;
                filter.Finished  = ParseBool(finishedStr);
                filter.DepartmentId = FixedDepartamentId;

                items = await OccurrenceService.SearchAsync(filter);

                pageNumber = 1;
                RecomputePages();
            }
            catch (Exception ex)
            {
                errorMessage = "Erro ao buscar ocorrências.";
                Console.WriteLine(ex);
            }
        }

        protected async Task ResetFilters()
        {
            filter = new OccurrenceSearchFilter();
            filter.DepartmentId = FixedDepartamentId;
            startDate = endDate = null;
            finishedStr = null;

            items = await OccurrenceService.SearchAsync(filter);

            pageNumber = 1;
            RecomputePages();
        }

        private static bool? ParseBool(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            return s.Equals("true", StringComparison.OrdinalIgnoreCase) ? true
                 : s.Equals("false", StringComparison.OrdinalIgnoreCase) ? false
                 : (bool?)null;
        }

        private void RecomputePages()
        {
            totalPages = Math.Max(1, (int)Math.Ceiling((double)items.Count / pageSize));
            if (pageNumber > totalPages) pageNumber = totalPages;
        }

        // ====== MODAL ======
        protected void OpenAddModal()
        {
            formError = null;
            isEditing = false;
            current = new OccurrenceModel
            {
                Process = new ReferenceEntity(),
                User = new ReferenceEntity(),
                OccurrenceType = new OccurrenceTypeModel(),
                Part = new ReferenceEntity()
            };
            selectedOccurrenceTypeId = string.Empty;
            selectedPartId = string.Empty;
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
                OccurrenceType = o.OccurrenceType is null ? new OccurrenceTypeModel() : new OccurrenceTypeModel
                {
                    Id = o.OccurrenceType.Id,
                    Name = o.OccurrenceType.Name,
                    Description = o.OccurrenceType.Description,
                    PendingEvent = o.OccurrenceType.PendingEvent
                },
                Part = new ReferenceEntity { Id = o.Part?.Id ?? "", Name = o.Part?.Name ?? "" }
            };

            selectedOccurrenceTypeId = current.OccurrenceType?.Id ?? "";
            selectedPartId = current.Part?.Id ?? "";
            showModal = true;
        }

        protected void CloseModal() => showModal = false;

        protected void OnTypeChanged()
        {
            var found = occurrenceTypes.FirstOrDefault(t => t.Id == selectedOccurrenceTypeId);
            current.OccurrenceType = found is null
                ? new OccurrenceTypeModel()
                : new OccurrenceTypeModel
                {
                    Id = found.Id,
                    Name = found.Name,
                    Description = found.Description,
                    PendingEvent = found.PendingEvent
                };
        }

        protected void OnPartChanged()
        {
            if (string.IsNullOrWhiteSpace(selectedPartId))
            {
                current.Part = new ReferenceEntity();
                return;
            }

            var p = parts.FirstOrDefault(x => x.Id == selectedPartId);
            current.Part = p is null ? new ReferenceEntity() : new ReferenceEntity { Id = p.Id, Name = p.Name };
        }

        protected async Task Save()
        {
            formError = null;

            if (string.IsNullOrWhiteSpace(current.OccurrenceType?.Id))
            {
                formError = "Selecione o Tipo de Ocorrência.";
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
                    if (created is null) throw new InvalidOperationException("Falha ao criar ocorrência.");
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

        // ====== Ações da tabela ======
        protected async Task Delete(string id)
        {
            try
            {
                var ok = await OccurrenceService.DeleteAsync(id);
                if (!ok) throw new InvalidOperationException("Falha ao excluir.");
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

                // Recarrega a lista para refletir Finished = true e EndDate preenchido
                await LoadAll();
            }
            catch (Exception ex)
            {
                errorMessage = "Erro ao finalizar ocorrência.";
                Console.WriteLine(ex);
            }
        }


        // ====== Paginação ======
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
