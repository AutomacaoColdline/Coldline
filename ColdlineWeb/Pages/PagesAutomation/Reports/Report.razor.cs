using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Blazorise.Charts;

namespace ColdlineWeb.Pages.PagesAutomation.Reports
{
    public partial class ReportPage : ComponentBase
    {
        [Inject] protected IJSRuntime JS { get; set; } = default!;

        protected bool isLoading = true;

        protected LineChart<double>? lineChart;
        protected BarChart<double>? barChart;
        protected PieChart<double>? pieChart;

        readonly List<string> dias = new() { "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb", "Dom" };
        readonly List<double> producao = new() { 10, 14, 18, 9, 15, 7, 12 };
        readonly List<double> vendas = new() { 8, 12, 16, 10, 18, 6, 9 };

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            await JS.InvokeVoidAsync("console.log", "🟢 Iniciando OnAfterRenderAsync");

            var tentativas = 0;
            while ((lineChart is null || barChart is null || pieChart is null) && tentativas < 10)
            {
                tentativas++;
                await JS.InvokeVoidAsync("console.log", $"⏳ Tentativa {tentativas}: lineChart={lineChart != null}, barChart={barChart != null}, pieChart={pieChart != null}");
                await Task.Delay(200);
                await InvokeAsync(StateHasChanged);
            }

            if (lineChart is null || barChart is null || pieChart is null)
            {
                await JS.InvokeVoidAsync("console.log", "❌ Charts ainda nulos após 10 tentativas. Abortando inicialização.");
                return;
            }

            await JS.InvokeVoidAsync("console.log", "✅ Todos os charts renderizados. Iniciando datasets...");

            // Linha
            await JS.InvokeVoidAsync("console.log", "➡️  Criando gráfico de linha");
            await lineChart.AddLabelsDatasetsAndUpdate(dias, new LineChartDataset<double>
            {
                Label = "Produção",
                Data = producao,
                BackgroundColor = "rgba(59,130,246,0.3)",
                BorderColor = "#3B82F6",
                Fill = true,
                Tension = 0.3f
            });

            // Barras
            await JS.InvokeVoidAsync("console.log", "➡️  Criando gráfico de barras");
            await barChart.AddLabelsDatasetsAndUpdate(dias, new BarChartDataset<double>
            {
                Label = "Vendas",
                Data = vendas,
                BackgroundColor = "#10B981"
            });

            // Pizza
            await JS.InvokeVoidAsync("console.log", "➡️  Criando gráfico de pizza");
            await pieChart.AddLabelsDatasetsAndUpdate(dias, new PieChartDataset<double>
            {
                Label = "Setores",
                Data = new List<double> { 12, 8, 15, 10, 9, 6, 14 },
                BackgroundColor = new List<string>
                {
                    "#F59E0B", "#3B82F6", "#EF4444", "#10B981",
                    "#8B5CF6", "#EC4899", "#06B6D4"
                }
            });

            await JS.InvokeVoidAsync("console.log", "🏁 Gráficos carregados com sucesso!");

            isLoading = false;
            await JS.InvokeVoidAsync("console.log", "🟢 isLoading = false, atualizando interface...");

            await InvokeAsync(StateHasChanged);
        }
    }
}
