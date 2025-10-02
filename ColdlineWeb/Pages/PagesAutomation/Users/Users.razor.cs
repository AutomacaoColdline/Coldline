using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;
using ColdlineWeb.Services;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;
using ColdlineWeb.Models.Common;

namespace ColdlineWeb.Pages.PagesAutomation.Users
{
    public partial class UsersPage : ComponentBase
    {
        [Inject] private UserService UserService { get; set; } = default!;
        [Inject] private HttpClient Http { get; set; } = default!;

        protected List<UserModel> users = new();
        protected List<ReferenceEntity> userTypes = new();
        protected List<ReferenceEntity> departments = new();

        // Filtros
        protected string? filterName;
        protected string? filterEmail;
        protected string? filterDepartmentId = null;
        protected string? filterUserTypeId;

        // Paginação
        protected int pageNumber = 1;
        protected int pageSize = 5;
        protected int totalPages;
        protected long totalCount;

        // Controle
        protected bool isLoading = true;
        protected bool showModal = false;
        protected bool isEditing = false;
        protected string? errorMessage;
        protected IBrowserFile? selectedFile;

        // Usuário corrente
        protected UserModel currentUser = new();

        // Modais
        protected bool showUserTypeModal = false;
        protected UserTypeModel newUserType = new();
        protected bool showDepartmentModal = false;
        protected DepartmentModel newDepartment = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadFilterData();
            await LoadData();
        }

        private async Task LoadFilterData()
        {
            try
            {
                userTypes = await UserService.GetUserTypesAsync();

                var deptModels = await Http.GetFromJsonAsync<List<DepartmentModel>>("api/Department")
                                 ?? new List<DepartmentModel>();
                departments = deptModels.ConvertAll(d => new ReferenceEntity { Id = d.Id, Name = d.Name });
            }
            catch
            {
                errorMessage = "Erro ao carregar dados de departamento/tipo.";
            }
        }

        protected async Task ApplyFilters()
        {
            pageNumber = 1;
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                isLoading = true;
                errorMessage = null;

                var filter = new UserFilterModel
                {
                    Name = filterName,
                    Email = filterEmail,
                    DepartmentId = filterDepartmentId,
                    UserTypeId = filterUserTypeId,
                    Page = pageNumber,
                    PageSize = pageSize
                };

                var result = await UserService.SearchUsersAsync(filter);

                // Ajuste de conversão
                users = result.Items?.ToList() ?? new List<UserModel>();

                // Ajuste de contadores conforme PagedResult
                totalPages = result.TotalPages;
                totalCount = result.Total; // ✅ uso correto da propriedade
            }
            catch
            {
                errorMessage = "Erro ao carregar dados.";
            }
            finally
            {
                isLoading = false;
            }
        }

        // Paginação
        protected bool CanGoPrevious => pageNumber > 1;
        protected bool CanGoNext => pageNumber < totalPages;

        protected async Task GoToPreviousPage()
        {
            if (CanGoPrevious)
            {
                pageNumber--;
                await LoadData();
            }
        }

        protected async Task GoToNextPage()
        {
            if (CanGoNext)
            {
                pageNumber++;
                await LoadData();
            }
        }

        // CRUD
        protected async Task SaveUser()
        {
            try
            {
                currentUser.Id = null;
                currentUser.UserType.Name = userTypes.Find(ut => ut.Id == currentUser.UserType.Id)?.Name ?? string.Empty;
                currentUser.Department.Name = departments.Find(d => d.Id == currentUser.Department.Id)?.Name ?? string.Empty;
                currentUser.UrlPhoto = $"{currentUser.Name}.png";

                if (selectedFile != null)
                {
                    string? uploadedFileName = await UserService.UploadImageAsync(selectedFile, string.Empty, currentUser.Name);
                    if (uploadedFileName == null)
                    {
                        errorMessage = "Erro ao enviar a imagem. Usuário não foi salvo.";
                        return;
                    }
                }
                else
                {
                    errorMessage = "Nenhuma imagem foi selecionada. O usuário não foi salvo.";
                    return;
                }

                bool success = await UserService.SaveUserAsync(currentUser);
                if (success)
                {
                    showModal = false;
                    await LoadData();
                }
                else
                {
                    errorMessage = "Erro ao salvar usuário.";
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Erro ao salvar usuário: {ex.Message}";
            }
        }

        protected async Task UpdateUser()
        {
            try
            {
                errorMessage = null;

                currentUser.UserType.Name = userTypes.Find(ut => ut.Id == currentUser.UserType.Id)?.Name ?? string.Empty;
                currentUser.Department.Name = departments.Find(d => d.Id == currentUser.Department.Id)?.Name ?? string.Empty;

                var oldUser = users.Find(u => u.Id == currentUser.Id);
                string oldFileName = oldUser?.Name ?? currentUser.Name;
                string newFileName = currentUser.Name;

                if (selectedFile != null)
                {
                    string? uploadedFileName = await UserService.UploadImageAsync(selectedFile, oldFileName, newFileName);
                    if (!string.IsNullOrEmpty(uploadedFileName))
                    {
                        currentUser.UrlPhoto = $"{currentUser.Name}.png";
                    }
                }
                else if (oldFileName != newFileName)
                {
                    currentUser.UrlPhoto = $"{currentUser.Name}.png";
                }

                bool success = await UserService.UpdateUserAsync(currentUser.Id, currentUser);
                if (success)
                {
                    showModal = false;
                    selectedFile = null;
                    await LoadData();
                }
                else
                {
                    errorMessage = "Erro ao atualizar usuário.";
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Erro inesperado ao atualizar usuário: {ex.Message}";
            }
        }

        protected async Task DeleteUser(string id)
        {
            bool success = await UserService.DeleteUserAsync(id);
            if (success)
            {
                await LoadData();
            }
            else
            {
                errorMessage = "Erro ao excluir usuário.";
            }
        }

        // Modais
        protected void OpenAddUserModal()
        {
            currentUser = new UserModel();
            showModal = true;
            isEditing = false;
            selectedFile = null;
        }

        protected void OpenEditUserModal(UserModel u)
        {
            currentUser = u;
            showModal = true;
            isEditing = true;
            selectedFile = null;
        }

        protected void CloseModal() => showModal = false;

        protected async Task UploadImage(InputFileChangeEventArgs e)
        {
            if (e.File == null)
            {
                errorMessage = "Nenhum arquivo foi selecionado.";
                return;
            }
            selectedFile = e.File;
        }

        // Tipo de Usuário
        protected void OpenUserTypeModal()
        {
            newUserType = new UserTypeModel();
            showUserTypeModal = true;
        }

        protected void CloseUserTypeModal() => showUserTypeModal = false;

        protected async Task SaveUserType()
        {
            try
            {
                await Http.PostAsJsonAsync("api/UserType", newUserType);
                userTypes = await UserService.GetUserTypesAsync();
                showUserTypeModal = false;
            }
            catch (Exception ex)
            {
                errorMessage = "Erro ao salvar o tipo de usuário.";
                Console.WriteLine(ex.Message);
            }
        }

        // Departamento
        protected void OpenDepartmentModal()
        {
            newDepartment = new DepartmentModel();
            showDepartmentModal = true;
        }

        protected void CloseDepartmentModal() => showDepartmentModal = false;

        protected async Task SaveDepartment()
        {
            try
            {
                await Http.PostAsJsonAsync("api/Department", newDepartment);

                var deptModels = await Http.GetFromJsonAsync<List<DepartmentModel>>("api/Department")
                                 ?? new List<DepartmentModel>();
                departments = deptModels.ConvertAll(d => new ReferenceEntity { Id = d.Id, Name = d.Name });
                showDepartmentModal = false;
            }
            catch (Exception ex)
            {
                errorMessage = "Erro ao salvar o departamento.";
                Console.WriteLine(ex.Message);
            }
        }
    }
}
