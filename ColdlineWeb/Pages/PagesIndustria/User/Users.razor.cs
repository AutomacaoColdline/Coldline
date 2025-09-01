using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;
using ColdlineWeb.Services;
using ColdlineWeb.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ColdlineWeb.Pages.PagesIndustria.User
{
    public partial class Users : ComponentBase
    {
        [Inject] private UserService UserService { get; set; } = default!;

        private List<UserModel> users = new();

        private List<ReferenceEntity> userTypes = new();
        private List<ReferenceEntity> departments = new();

        // Filtros
        private string? filterName;
        private string? filterEmail;
        private string? filterDepartmentId = "67f41c323a50bfa4e95bfe6d";
        private string? filterUserTypeId;

        // Paginação
        private int pageNumber = 1;
        private int pageSize = 5; // Exemplo de tamanho de página
        private int totalPages;
        private long totalCount;

        // Controle da tela
        private bool isLoading = true;
        private bool showModal = false;
        private bool isEditing = false;
        private string? errorMessage;
        private IBrowserFile? selectedFile;

        // Usuário corrente para Add/Edit
        private UserModel currentUser = new();

        // Controle do modal de Tipo de Usuário
        private bool showUserTypeModal = false; 
        private UserTypeModel newUserType = new UserTypeModel();

        // Controle do modal de Departamento
        private bool showDepartmentModal = false;
        private DepartmentModel newDepartment = new DepartmentModel();

        protected override async Task OnInitializedAsync()
        {
            // Carrega dropdowns (faz a conversão para departamentos)
            await LoadFilterData();

            // Carrega usuários (página 1, sem filtros iniciais)
            await LoadData();
        }

        /// <summary>
        /// Carrega dados para popular departamentos e tipos de usuário.
        /// </summary>
        private async Task LoadFilterData()
        {
            try
            {
                userTypes = await UserService.GetUserTypesAsync();
                // Obtém os departamentos como DepartmentModel e converte para ReferenceEntity
                var deptModels = await Http.GetFromJsonAsync<List<DepartmentModel>>("api/Department") 
                                 ?? new List<DepartmentModel>();
                departments = deptModels.ConvertAll(d => new ReferenceEntity { Id = d.Id, Name = d.Name });
            }
            catch
            {
                errorMessage = "Erro ao carregar dados de departamento/tipo.";
            }
        }

        /// <summary>
        /// Aplica filtros: redefine página para 1 e busca novamente.
        /// </summary>
        private async Task ApplyFilters()
        {
            pageNumber = 1;
            await LoadData();
        }

        /// <summary>
        /// Busca usuários usando o método paginado SearchUsersAsync.
        /// </summary>
        private async Task LoadData()
        {
            try
            {
                isLoading = true;
                errorMessage = null;

                var result = await UserService.SearchUsersAsync(
                    filterName,
                    filterEmail,
                    filterDepartmentId,
                    filterUserTypeId,
                    pageNumber,
                    pageSize
                );

                users = result.Items;
                totalPages = result.TotalPages;
                totalCount = result.TotalCount;
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

        // ================ Métodos de Paginação =================
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

        // ================ CRUD (Add/Edit/Delete) ================
        private async Task SaveUser()
        {
            try
            {
                currentUser.Id = null;
                currentUser.UserType.Name = userTypes.Find(ut => ut.Id == currentUser.UserType.Id)?.Name ?? "";
                // Como o departamento em UserModel é uma ReferenceEntity, fazemos a busca na lista "departments"
                currentUser.Department.Name = departments.Find(d => d.Id == currentUser.Department.Id)?.Name ?? "";
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

                currentUser.UserType.Name = userTypes.Find(ut => ut.Id == currentUser.UserType.Id)?.Name ?? "";
                currentUser.Department.Name = departments.Find(d => d.Id == currentUser.Department.Id)?.Name ?? "";

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

        private async Task DeleteUser(string id)
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

        private void OpenAddUserModal()
        {
            currentUser = new UserModel();
            showModal = true;
            isEditing = false;
            selectedFile = null;
        }

        private void OpenEditUserModal(UserModel user)
        {
            currentUser = user;
            showModal = true;
            isEditing = true;
            selectedFile = null;
        }

        private void CloseModal()
        {
            showModal = false;
        }

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

        // Métodos para o modal de Tipo de Usuário
        private void OpenUserTypeModal()
        {
            newUserType = new UserTypeModel();
            showUserTypeModal = true;
        }

        private void CloseUserTypeModal()
        {
            showUserTypeModal = false;
        }

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

        // Métodos para o modal de Departamento
        private void OpenDepartmentModal()
        {
            newDepartment = new DepartmentModel();
            showDepartmentModal = true;
        }

        private void CloseDepartmentModal()
        {
            showDepartmentModal = false;
        }

        private async Task SaveDepartment()
        {
            try
            {
                await Http.PostAsJsonAsync("api/Department", newDepartment);
                // Recarrega os departamentos e converte para ReferenceEntity
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
