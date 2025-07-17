using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using ColdlineWeb.Models;

namespace ColdlineWeb.Pages
{
    public class HomeAssistenciaPage : ComponentBase
    {
        [Inject] protected HttpClient Http { get; set; } = default!;

        protected bool showEditModal = false;
        protected string activeSection = "acessos";

        protected List<ArquivoInfoModel> arquivos = new();
        protected List<MonitoringModel> monitoramentos = new();
        protected MonitoringFilterModel filtro = new();
        protected List<string> estadosDisponiveis = new();
        protected List<string> cidadesDisponiveis = new();
        protected List<MonitoringTypeModel> tiposMonitoramento = new();

        protected MonitoringModel? currentEditing;
        protected bool showViewModal = false;
        protected string senhaDigitada = string.Empty;
        protected bool showPasswordModal = false;
        protected MonitoringModel? monitoramentoParaEditar;
        private MonitoringModel? originalEditing;

        protected void ShowAcessos() => activeSection = "acessos";
        protected void ShowClientes() => activeSection = "clientes";

        protected async Task ShowDocumentacao()
        {
            activeSection = "documentacao";
            await CarregarArquivos();
        }

        protected override async Task OnInitializedAsync()
        {
            await CarregarTiposMonitoramento();
            await BuscarMonitoramentos();
            AtualizarListasDeFiltros();
        }
         protected void Visualizar(MonitoringModel monitoramento)
        {
            currentEditing = monitoramento;
            showViewModal = true;
        }

        protected async Task BuscarMonitoramentos()
        {
            try
            {
                filtro.Page = 1;
                filtro.PageSize = 100;

                var response = await Http.PostAsJsonAsync("http://10.0.0.44:5000/api/Monitoring/filter", filtro);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<MonitoringModel>>();
                    monitoramentos = data ?? new();

                    AtualizarListasDeFiltros();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar monitoramentos: {ex.Message}");
            }
            finally
            {
                filtro.Estado = "";
                filtro.Cidade = "";
            }
        }

        private void AtualizarListasDeFiltros()
        {
            estadosDisponiveis = monitoramentos
                .Select(m => m.Estado?.Trim().ToUpper())
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct()
                .OrderBy(e => e)
                .ToList();

            cidadesDisponiveis = monitoramentos
                .Select(m => m.Cidade?.Trim())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }

        protected async Task CarregarArquivos()
        {
            try
            {
                var response = await Http.GetFromJsonAsync<List<ArquivoInfoModel>>("http://10.0.0.44:5000/api/Arquivo");
                if (response != null)
                    arquivos = response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar arquivos: {ex.Message}");
            }
        }

         protected async Task CarregarTiposMonitoramento()
        {
            try
            {
                var result = await Http.GetFromJsonAsync<List<MonitoringTypeModel>>("http://10.0.0.44:5000/api/MonitoringType");
                if (result != null)
                    tiposMonitoramento = result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao carregar tipos: " + ex.Message);
            }
        }

        protected void Editar(MonitoringModel monitoramento)
        {
            senhaDigitada = string.Empty;
            monitoramentoParaEditar = monitoramento;
            // Clonar objeto para comparar depois
            originalEditing = new MonitoringModel
            {
                Id = monitoramento.Id,
                Cidade = monitoramento.Cidade,
                Clp = new List<string>(monitoramento.Clp),
                Estado = monitoramento.Estado,
                Gateway = monitoramento.Gateway,
                Ihm = monitoramento.Ihm,
                Identificador = monitoramento.Identificador,
                IdAnydesk = monitoramento.IdAnydesk,
                IdRustdesk = monitoramento.IdRustdesk,
                IdTeamViewer = monitoramento.IdTeamViewer,
                Macs = new List<string>(monitoramento.Macs),
                Masc = monitoramento.Masc,
                Unidade = monitoramento.Unidade,
                MonitoringType = monitoramento.MonitoringType // se possível, Id também
            };

            showPasswordModal = true;
        }

        protected void ConfirmarSenha(string senha)
        {
            senhaDigitada = senha;
            if (senhaDigitada == "1234")
            {
                currentEditing = monitoramentoParaEditar;
                showEditModal = true;
                showPasswordModal = false;
            }
            else
            {
                // Você pode exibir um alerta ou limpar a senha
                Console.WriteLine(senhaDigitada);
                senhaDigitada = string.Empty;
                Console.WriteLine("Senha incorreta.");
            }
        }

        protected void FecharModal()
        {
            showViewModal = false;
            showEditModal = false;
            currentEditing = null;
            monitoramentoParaEditar = null;
            senhaDigitada = string.Empty;
        }

        protected async Task SalvarAlteracoes()
        {
            if (currentEditing == null || originalEditing == null)
                return;

            // Se nenhum campo relevante foi alterado
            bool semAlteracoes = 
                currentEditing.Identificador == originalEditing.Identificador &&
                currentEditing.Unidade == originalEditing.Unidade &&
                currentEditing.Estado == originalEditing.Estado &&
                currentEditing.Cidade == originalEditing.Cidade &&
                currentEditing.Ihm == originalEditing.Ihm &&
                currentEditing.Masc == originalEditing.Masc &&
                currentEditing.Gateway == originalEditing.Gateway &&
                currentEditing.IdAnydesk == originalEditing.IdAnydesk &&
                currentEditing.IdRustdesk == originalEditing.IdRustdesk &&
                currentEditing.IdTeamViewer == originalEditing.IdTeamViewer &&
                currentEditing.MonitoringType?.Id == originalEditing.MonitoringType?.Id &&
                Enumerable.SequenceEqual(currentEditing.Clp ?? new(), originalEditing.Clp ?? new()) &&
                Enumerable.SequenceEqual(currentEditing.Macs ?? new(), originalEditing.Macs ?? new());

            if (semAlteracoes)
            {
                Console.WriteLine("Nada foi alterado.");
                FecharModal(); // apenas fecha o modal
                return;
            }

            try
            {
                var response = await Http.PutAsJsonAsync($"http://10.0.0.44:5000/api/Monitoring/{currentEditing.Id}", currentEditing);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Monitoramento atualizado com sucesso.");
                    await BuscarMonitoramentos();
                    FecharModal();
                }
                else
                {
                    Console.WriteLine($"Erro ao atualizar: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na solicitação PUT: {ex.Message}");
            }
        }

    }
}
