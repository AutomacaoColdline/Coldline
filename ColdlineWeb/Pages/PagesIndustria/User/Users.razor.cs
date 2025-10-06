using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;
using ColdlineWeb.Services;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Filter;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using ColdlineWeb.Util;

namespace ColdlineWeb.Pages.PagesIndustria.User
{
    public partial class Users : ComponentBase
    {
        [Inject] private UserService UserService { get; set; } = default!;
        [Inject] private HttpClient Http { get; set; } = default!;

        private List<UserModel> users = new List<UserModel>();
        private List<ReferenceEntity> userTypes = new List<ReferenceEntity>();
        private List<ReferenceEntity> departments = new List<ReferenceEntity>();

        // Filtros
        private string? filterName;
        private string? filterEmail;
        private string? filterDepartmentId = EnvironmentHelper.GetDepartmentIdIndustria();
        private string? filterUserTypeId;

        // Paginação
        private int pageNumber = 1;
        private int pageSize = 5;
        private int totalPages;
        private long totalCount;

        // Controle da tela
        private bool isLoading = true;
        private bool showModal = false;
        private bool isEditing = false;
        private string? errorMessage;
        private IBrowserFile? selectedFile;

        // Usuário corrente para Add/Edit
        private UserModel currentUser = new UserModel();

        // Controle do modal de Tipo de Usuário
        private bool showUserTypeModal = false;
        private UserTypeModel newUserType = new UserTypeModel();

        // Controle do modal de Departamento
        private bool showDepartmentModal = false;
        private DepartmentModel newDepartment = new DepartmentModel();

        // Controle do modal de Calendário
        private bool showCalendarModal = false;
        private UserModel currentCalendarUser = new UserModel();
        private string? selectedCalendarUserId;

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

        private async Task ApplyFilters()
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

                users = result?.Items?.ToList() ?? new List<UserModel>();
                totalCount = result?.Total ?? 0;
                totalPages = result != null ? (int)Math.Ceiling((double)result.Total / pageSize) : 0;
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
        private bool CanGoPrevious => pageNumber > 1;
        private bool CanGoNext => pageNumber < totalPages;

        private async Task GoToPreviousPage()
        {
            if (CanGoPrevious)
            {
                pageNumber--;
                await LoadData();
            }
        }

        private async Task GoToNextPage()
        {
            if (CanGoNext)
            {
                pageNumber++;
                await LoadData();
            }
        }

        // CRUD Usuário
        private async Task SaveUser()
        {
            try
            {
                currentUser.Id = null;

                // Correção: criar objetos do tipo ReferenceEntity
                currentUser.UserType ??= new ReferenceEntity();
                currentUser.Department ??= new ReferenceEntity();

                currentUser.UserType.Name = userTypes.FirstOrDefault(ut => ut.Id == currentUser.UserType.Id)?.Name ?? "";
                currentUser.Department.Name = departments.FirstOrDefault(d => d.Id == currentUser.Department.Id)?.Name ?? "";

                currentUser.UrlPhoto = $"{currentUser.Name}.png";

                if (selectedFile != null)
                {
                    string? uploadedFileName = await UserService.UploadImageAsync(selectedFile, "", currentUser.Name);
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
                    selectedFile = null;
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

        private async Task UpdateUser()
        {
            try
            {
                errorMessage = null;

                currentUser.UserType ??= new ReferenceEntity();
                currentUser.Department ??= new ReferenceEntity();

                currentUser.UserType.Name = userTypes.FirstOrDefault(ut => ut.Id == currentUser.UserType.Id)?.Name ?? "";
                currentUser.Department.Name = departments.FirstOrDefault(d => d.Id == currentUser.Department.Id)?.Name ?? "";

                var oldUser = users.FirstOrDefault(u => u.Id == currentUser.Id);
                string oldFileName = oldUser?.Name ?? currentUser.Name;
                string newFileName = currentUser.Name;

                if (selectedFile != null)
                {
                    string? uploadedFileName = await UserService.UploadImageAsync(selectedFile, oldFileName, newFileName);
                    if (!string.IsNullOrEmpty(uploadedFileName))
                        currentUser.UrlPhoto = $"{currentUser.Name}.png";
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

        private async Task DeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                errorMessage = "Id do usuário inválido.";
                return;
            }

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

        private void OpenAddUserModal()
        {
            currentUser = new UserModel();
            showModal = true;
            isEditing = false;
            selectedFile = null;
        }

        private void OpenEditUserModal(UserModel user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Id))
            {
                errorMessage = "Usuário inválido para edição.";
                return;
            }

            currentUser = user;
            showModal = true;
            isEditing = true;
            selectedFile = null;
        }

        private void CloseModal() => showModal = false;

        private async Task UploadImage(InputFileChangeEventArgs e)
        {
            if (e.File == null)
            {
                errorMessage = "Nenhum arquivo foi selecionado.";
                return;
            }
            selectedFile = e.File;
            Console.WriteLine($"Arquivo selecionado: {selectedFile.Name}, Tamanho: {selectedFile.Size} bytes");
        }

        // Modal Tipo de Usuário
        private void OpenUserTypeModal()
        {
            newUserType = new UserTypeModel();
            showUserTypeModal = true;
        }

        private void CloseUserTypeModal() => showUserTypeModal = false;

        private async Task SaveUserType()
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

        // Modal Departamento
        private void OpenDepartmentModal()
        {
            newDepartment = new DepartmentModel();
            showDepartmentModal = true;
        }

        private void CloseDepartmentModal() => showDepartmentModal = false;

        private async Task SaveDepartment()
        {
            try
            {
                await Http.PostAsJsonAsync("api/Department", newDepartment);
                var deptModels = await Http.GetFromJsonAsync<List<DepartmentModel>>("api/Department") ?? new List<DepartmentModel>();
                departments = deptModels.ConvertAll(d => new ReferenceEntity { Id = d.Id, Name = d.Name });
                showDepartmentModal = false;
            }
            catch (Exception ex)
            {
                errorMessage = "Erro ao salvar o departamento.";
                Console.WriteLine(ex.Message);
            }
        }

        // Modal Calendário
        private void OpenCalendarModal(UserModel user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Id))
            {
                errorMessage = "Id do usuário inválido para o calendário.";
                return;
            }

            currentCalendarUser = user;
            selectedCalendarUserId = user.Id;
            showCalendarModal = true;
        }

        private void CloseCalendarModal()
        {
            showCalendarModal = false;
            selectedCalendarUserId = null;
            currentCalendarUser = new UserModel();
        }
    }
}
