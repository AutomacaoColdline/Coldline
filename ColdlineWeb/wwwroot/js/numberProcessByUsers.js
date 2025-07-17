function renderizarGraficoUsuarios(labels, values, chartType) {
    const ctx = document.getElementById('processChart').getContext('2d');
    if (window.graficoUsuarios) {
        window.graficoUsuarios.destroy();
    }

    window.graficoUsuarios = new Chart(ctx, {
        type: chartType,
        data: {
            labels: labels,
            datasets: [{
                label: 'Quantidade de Processos',
                data: values,
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
}
