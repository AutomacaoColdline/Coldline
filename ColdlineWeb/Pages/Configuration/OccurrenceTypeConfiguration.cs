using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ColdlineWeb.Models;
using ColdlineWeb.Services;

namespace ColdlineWeb.Pages
{
    public partial class ConfigurationPage
    {
        // ---- Estado do modal (iguais aos outros tabs) ----
        protected bool showOccurrenceTypeModal = false;
        protected bool isEditingOccurrenceType = false;
        protected OccurrenceTypeModel currentOccurrenceType = new();

        // ---- Paginação da aba ----
        protected int OccurrenceTypePageNumber = 1;
        protected int OccurrenceTypePageSize = 5;

        protected List<OccurrenceTypeModel> PagedOccurrenceTypes =>
            (OccurrenceTypes ?? new())
                .Skip((OccurrenceTypePageNumber - 1) * OccurrenceTypePageSize)
                .Take(OccurrenceTypePageSize)
                .ToList();

        protected int TotalOccurrenceTypePages =>
            (int)Math.Ceiling(((double)(OccurrenceTypes?.Count ?? 0)) / OccurrenceTypePageSize);

        protected void GoToPreviousOccurrenceTypePage()
        {
            if (OccurrenceTypePageNumber > 1)
                OccurrenceTypePageNumber--;
        }

        protected void GoToNextOccurrenceTypePage()
        {
            if (OccurrenceTypePageNumber < TotalOccurrenceTypePages)
                OccurrenceTypePageNumber++;
        }

        // ---- Abrir modais ----
        protected void OpenAddOccurrenceTypeModal()
        {
            currentOccurrenceType = new OccurrenceTypeModel();
            isEditingOccurrenceType = false;
            showOccurrenceTypeModal = true;
        }

        protected void OpenEditOccurrenceTypeModal(OccurrenceTypeModel item)
        {
            currentOccurrenceType = new OccurrenceTypeModel
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                PendingEvent = item.PendingEvent
            };
            isEditingOccurrenceType = true;
            showOccurrenceTypeModal = true;
        }

        // ---- Excluir ----
        protected async Task DeleteOccurrenceType(string id)
        {
            var confirm = await JS.InvokeAsync<bool>("confirm", new object[] { "Tem certeza que deseja excluir este tipo de ocorrência?" });
            if (!confirm) return;

            var success = await OccurrenceTypeService.DeleteAsync(id);
            if (success)
            {
                await LoadAll();

                // se a página ficou vazia após o delete, recua 1
                if (OccurrenceTypePageNumber > 1 && !PagedOccurrenceTypes.Any())
                    OccurrenceTypePageNumber--;
            }
        }
    }
}
