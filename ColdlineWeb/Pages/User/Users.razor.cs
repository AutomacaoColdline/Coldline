using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using ColdlineWeb.Services;
using ColdlineWeb.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineWeb.Pages.User
{
    public partial class Users : ComponentBase
    {
        [Inject] private UserService UserService { get; set; } = default!;

        private List<UserModel> users = new();
        private List<ReferenceEntity> userTypes = new();
        private List<ReferenceEntity> departments = new();
        private bool isLoading = true;
        private bool showModal = false;
        private bool isEditing = false;
        private string? errorMessage;
        private IBrowserFile? selectedFile;
        private UserModel currentUser = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                users = await UserService.GetUsersAsync();
                userTypes = await UserService.GetUserTypesAsync();
                departments = await UserService.GetDepartmentsAsync();
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

        private async Task SaveUser()
        {
            currentUser.Id = null;
            currentUser.UserType.Name = userTypes.Find(ut => ut.Id == currentUser.UserType.Id)?.Name ?? "";
            currentUser.Department.Name = departments.Find(d => d.Id == currentUser.Department.Id)?.Name ?? "";
            currentUser.UrlPhoto = currentUser.Name + ".png";
            
            // ✅ 1. Salvar usuário primeiro
            bool success = await UserService.SaveUserAsync(currentUser);
            
            if (success)
            {
                string? uploadedFileName = await UserService.UploadImageAsync(selectedFile, currentUser.Name);
                if (uploadedFileName == null)
                {
                    errorMessage = "Erro ao enviar a imagem.";
                }

                showModal = false;
                await LoadData();
            }
            else
            {
                errorMessage = "Erro ao salvar usuário.";
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
        }

        private void OpenEditUserModal(UserModel user)
        {
            currentUser = user;
            showModal = true;
            isEditing = true;
        }

        private void CloseModal()
        {
            showModal = false;
        }

        private async Task UploadImage(InputFileChangeEventArgs e)
        {
            var file = e.File;
            if (file == null)
            {
                errorMessage = "Nenhum arquivo foi selecionado.";
                return;
            }

            string? uploadedFileName = await UserService.UploadImageAsync(file, currentUser.Name);
            if (uploadedFileName != null)
            {
                currentUser.UrlPhoto = uploadedFileName;
            }
            else
            {
                errorMessage = "Erro ao enviar a imagem.";
            }
        }
    }
}
