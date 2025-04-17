window.renderizarGraficoTempoPorTipoDeMaquina = (labels, data, type) => {
    const ctx = document.getElementById('machineTypeChart').getContext('2d');

    if (window.machineTypeChart instanceof Chart) {
        window.machineTypeChart.destroy();
    }

    window.machineTypeChart = new Chart(ctx, {
        type: type,
        data: {
            labels: labels,
            datasets: [{
                label: 'Tempo Médio por Tipo de Máquina (minutos)',
                data: data,
                backgroundColor: 'rgba(255, 206, 86, 0.5)',
                borderColor: 'rgba(255, 206, 86, 1)',
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
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Minutos'
                    }
                }
            }
        }
    });
};
