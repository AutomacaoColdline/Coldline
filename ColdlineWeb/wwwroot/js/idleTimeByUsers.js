window.renderizarGraficoTempoOciosoUsuarios = (labels, data, type) => {
    const ctx = document.getElementById('idleTimeChart').getContext('2d');

    if (window.idleTimeChart instanceof Chart) {
        window.idleTimeChart.destroy();
    }

    window.idleTimeChart = new Chart(ctx, {
        type: type,
        data: {
            labels: labels,
            datasets: [{
                label: 'Tempo Ocioso por Usuário (minutos)',
                data: data,
                backgroundColor: 'rgba(255, 193, 7, 0.5)',
                borderColor: 'rgba(255, 193, 7, 1)',
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
