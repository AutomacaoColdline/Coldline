window.renderizarGraficoOcorrenciasUsuarios = (labels, data, type) => {
    const ctx = document.getElementById('ocurrenceChart').getContext('2d');

    if (window.ocurrenceChart instanceof Chart) {
        window.ocurrenceChart.destroy();
    }

    window.ocurrenceChart = new Chart(ctx, {
        type: type,
        data: {
            labels: labels,
            datasets: [{
                label: 'Ocorrências por Usuário',
                data: data,
                backgroundColor: 'rgba(255, 99, 132, 0.5)',
                borderColor: 'rgba(255, 99, 132, 1)',
                borderWidth: 1,
                fill: type === 'line' ? false : true,
                tension: 0.3
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { display: true },
                tooltip: {
                    mode: 'index',
                    intersect: false
                }
            },
            scales: {
                x: {
                    ticks: {
                        maxRotation: 45,
                        minRotation: 45
                    }
                },
                y: {
                    beginAtZero: true
                }
            }
        }
    });
};
