using Microsoft.AspNetCore.Components;
using ColdlineWeb.Models;
using ColdlineWeb.Models.Enum;
using ColdlineWeb.Models.Filter;
using ColdlineWeb.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.JSInterop;

// >>> IMPORTANTE: adiciona o namespace do componente NoteViewer
using ColdlineWeb.Pages.PagesAutomation.Components;

namespace ColdlineWeb.Pages.PagesAutomation
{
    public partial class AutomationHomePage : ComponentBase
    {
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private UserService UserService { get; set; } = default!;
        [Inject] private NoteService NoteService { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;

        [Parameter] public string? Section { get; set; }

        protected UserModel? user;
        protected string? errorMessage;
        protected bool isLoading = true;

        protected List<NoteModel> notes = new();
        protected NoteFilterModel filterModel = new();
        protected List<NoteType> noteTypeOptions = Enum.GetValues(typeof(NoteType)).Cast<NoteType>().ToList();
        protected string? selectedNoteTypeStr;

        // ===== Overlays =====
        protected bool showDeleteConfirm = false;
        protected NoteModel? notePendingDelete;

        // ===== NoteViewer (Create/View/Edit) =====
        protected bool viewerVisible = false;
        protected NoteViewer.ViewerMode viewerMode = NoteViewer.ViewerMode.Create;
        protected NoteModel? viewerNote = null;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var uri = new Uri(Navigation.Uri);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var identificationNumber = query.Get("identificationNumber");

                if (string.IsNullOrWhiteSpace(identificationNumber))
                {
                    Navigation.NavigateTo("/automation/login", replace: true);
                    return;
                }

                await LoadUserAsync(identificationNumber);
                await LoadAllNotesAsync();
            }
            catch
            {
                errorMessage = "Erro ao carregar dados do usuário.";
            }
            finally
            {
                isLoading = false;
            }
        }

        // ===== Visualizar / Criar / Editar =====
        protected void OpenCreateNote()
        {
            viewerMode = NoteViewer.ViewerMode.Create;
            viewerNote = null;
            viewerVisible = true;
        }

        protected void OpenViewNote(NoteModel note)
        {
            viewerMode = NoteViewer.ViewerMode.View;
            viewerNote = note;
            viewerVisible = true;
        }

        protected void CloseViewer()
        {
            viewerVisible = false;
            viewerNote = null;
        }

        protected async Task HandleSaveNote(NoteModel newNote)
        {
            isLoading = true;
            StateHasChanged();

            try
            {
                var created = await NoteService.CreateAsync(newNote);
                if (created == null)
                {
                    errorMessage = "Não foi possível salvar a nota.";
                }
                else
                {
                    await ReloadAfterMutationAsync();
                }
            }
            catch
            {
                errorMessage = "Erro ao salvar a nota.";
            }
            finally
            {
                viewerVisible = false;
                isLoading = false;
            }
        }

        protected async Task HandleUpdateNote(NoteModel edited)
        {
            if (viewerNote?.Id is null)
            {
                errorMessage = "Registro inválido para atualização.";
                return;
            }

            isLoading = true;
            StateHasChanged();

            try
            {
                var ok = await NoteService.UpdateAsync(viewerNote.Id, edited);
                if (!ok)
                {
                    errorMessage = "Não foi possível atualizar a nota.";
                }
                else
                {
                    await ReloadAfterMutationAsync();
                }
            }
            catch
            {
                errorMessage = "Erro ao atualizar a nota.";
            }
            finally
            {
                viewerVisible = false;
                isLoading = false;
            }
        }

        private async Task ReloadAfterMutationAsync()
        {
            var hasAnyFilter =
                !string.IsNullOrWhiteSpace(filterModel.Name) ||
                !string.IsNullOrWhiteSpace(filterModel.Element) ||
                filterModel.NoteType.HasValue;

            if (hasAnyFilter)
            {
                var filteredNotes = await NoteService.SearchAsync(filterModel);
                notes = filteredNotes ?? new List<NoteModel>();
            }
            else
            {
                await LoadAllNotesAsync();
            }
        }

        // ===== Utilitários =====
        protected async Task CopyElement(string text)
        {
            try
            {
                var ok = await JS.InvokeAsync<bool>("copyToClipboard", text);
                if (!ok)
                {
                    errorMessage = "Não foi possível copiar o conteúdo.";
                }
            }
            catch
            {
                errorMessage = "Falha ao copiar o conteúdo.";
            }
        }

        private async Task LoadUserAsync(string identificationNumber)
        {
            user = await UserService.GetUserByIdentificationAsync(identificationNumber);
            if (user == null)
            {
                errorMessage = "Usuário não encontrado.";
            }
        }

        private async Task LoadAllNotesAsync()
        {
            try
            {
                var notesResult = await NoteService.GetAllAsync();
                notes = notesResult ?? new List<NoteModel>();
            }
            catch
            {
                errorMessage ??= "Falha ao carregar as notas.";
            }
        }

        protected async Task ApplyFilter()
        {
            isLoading = true;
            StateHasChanged();

            try
            {
                if (string.IsNullOrWhiteSpace(selectedNoteTypeStr))
                {
                    filterModel.NoteType = null;
                }
                else if (int.TryParse(selectedNoteTypeStr, out var noteTypeInt)
                         && Enum.IsDefined(typeof(NoteType), noteTypeInt))
                {
                    filterModel.NoteType = (NoteType)noteTypeInt;
                }
                else
                {
                    filterModel.NoteType = null;
                }

                var hasAnyFilter =
                    !string.IsNullOrWhiteSpace(filterModel.Name) ||
                    !string.IsNullOrWhiteSpace(filterModel.Element) ||
                    filterModel.NoteType.HasValue;

                if (!hasAnyFilter)
                {
                    await LoadAllNotesAsync();
                }
                else
                {
                    var filteredNotes = await NoteService.SearchAsync(filterModel);
                    notes = filteredNotes ?? new List<NoteModel>();
                }
            }
            catch
            {
                errorMessage = "Falha ao aplicar o filtro.";
            }
            finally
            {
                isLoading = false;
            }
        }

        protected async Task ClearFilter()
        {
            isLoading = true;
            StateHasChanged();

            try
            {
                filterModel = new NoteFilterModel();
                selectedNoteTypeStr = null;
                await LoadAllNotesAsync();
            }
            catch
            {
                errorMessage = "Falha ao limpar o filtro.";
            }
            finally
            {
                isLoading = false;
            }
        }

        // ===== Exclusão =====
        protected void OpenDeleteNote(NoteModel note)
        {
            notePendingDelete = note;
            showDeleteConfirm = true;
        }

        protected void CloseDeleteNote()
        {
            showDeleteConfirm = false;
            notePendingDelete = null;
        }

        protected async Task ConfirmDeleteNote()
        {
            if (notePendingDelete?.Id == null)
            {
                CloseDeleteNote();
                return;
            }

            isLoading = true;
            StateHasChanged();

            try
            {
                var deleted = await NoteService.DeleteAsync(notePendingDelete.Id);
                if (!deleted)
                {
                    errorMessage = "Não foi possível excluir a nota.";
                }
                else
                {
                    await ReloadAfterMutationAsync();
                }
            }
            catch
            {
                errorMessage = "Erro ao excluir a nota.";
            }
            finally
            {
                showDeleteConfirm = false;
                notePendingDelete = null;
                isLoading = false;
            }
        }

        // ===== Navegação =====
        protected void Logout() => Navigation.NavigateTo("/automation/login", replace: true);

        protected void HandleNavigate(string key)
        {
            var uri = new Uri(Navigation.Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var identificationNumber = query.Get("identificationNumber") ?? string.Empty;

            var path = string.IsNullOrWhiteSpace(key) || key == "home"
                ? "/automation"
                : $"/automation/{key}";

            var target = string.IsNullOrWhiteSpace(identificationNumber)
                ? path
                : $"{path}?identificationNumber={identificationNumber}";

            Navigation.NavigateTo(target, replace: false);
        }

        // ===== Helpers =====
        protected string GetNoteTypeDisplay(NoteType type)
        {
            var member = typeof(NoteType).GetMember(type.ToString()).FirstOrDefault();
            var display = member?.GetCustomAttribute<DisplayAttribute>();
            return display?.Name ?? type.ToString();
        }

        protected string GetNoteTypeCss(NoteType type)
        {
            return type switch
            {
                NoteType.Passwords       => "bg-dark",
                NoteType.DockerCommands  => "bg-info",
                NoteType.LinuxCommands   => "bg-success",
                NoteType.WindowsCommands => "bg-primary",
                NoteType.Notices         => "bg-warning text-dark",
                NoteType.Reminders       => "bg-secondary",
                _                        => "bg-light text-dark"
            };
        }
    }
}
