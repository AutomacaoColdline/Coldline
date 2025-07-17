console.log("✅ Dashboard.js carregado com sucesso.");

window.dashboardCharts = {
    renderGraficoLinha: function (labels, data) {
        console.log("📊 renderGraficoLinha chamado", { labels, data });

        const ctx = document.getElementById('graficoLinhaMaquinasIndividuais');
        if (!ctx) {
            console.warn("⚠️ Canvas 'graficoLinhaMaquinasIndividuais' não encontrado.");
            return;
        }

        const chartCtx = ctx.getContext('2d');

        if (window.graficoLinhaInstance instanceof Chart) {
            window.graficoLinhaInstance.destroy();
        }

        window.graficoLinhaInstance = new Chart(chartCtx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Tempo Total por Máquina (min)',
                    data: labels.map((_, i) => Number(data[i])),
                    fill: false,
                    borderColor: 'rgba(75, 192, 192, 1)',
                    backgroundColor: 'rgba(75, 192, 192, 0.2)',
                    tension: 0.3
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: { display: true },
                    tooltip: {
                        callbacks: {
                            label: function (ctx) {
                                const min = ctx.parsed.y;
                                const h = Math.floor(min / 60);
                                const m = Math.floor(min % 60);
                                return `${h}h ${m}min`;
                            }
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Tempo (h)'
                        },
                        ticks: {
                            callback: function (value) {
                                const h = Math.floor(value / 60);
                                const m = Math.floor(value % 60);
                                if (m === 0) return `${h}h`;
                                return `${h}h ${m}min`;
                            }
                        }
                    }
                }
            }
        });
    },

    renderGraficoBarra: function (canvasId, labels, data, label) {
        console.log(`📊 renderGraficoBarra chamado para '${canvasId}'`, { labels, data, label });

        const ctx = document.getElementById(canvasId);
        if (!ctx) {
            console.warn(`⚠️ Canvas '${canvasId}' não encontrado.`);
            return;
        }

        const chartCtx = ctx.getContext('2d');

        if (window[canvasId] instanceof Chart) {
            window[canvasId].destroy();
        }

        window[canvasId] = new Chart(chartCtx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: label,
                    data: labels.map((_, i) => Number(data[i])),
                    backgroundColor: 'rgba(54, 162, 235, 0.5)',
                    borderColor: 'rgba(54, 162, 235, 1)',
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                scales: {
                    y: {
                        beginAtZero: true
                    }
                },
                plugins: {
                    legend: { display: true }
                }
            }
        });
    }
};
