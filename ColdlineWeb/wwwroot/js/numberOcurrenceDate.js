window.renderizarGraficoOcurrence = (labels, data, type) => {
    const ctx = document.getElementById('ocurrenceChart').getContext('2d');

    if (window.ocurrenceChart instanceof Chart) {
        window.ocurrenceChart.destroy();
    }

    window.ocurrenceChart = new Chart(ctx, {
        type: type,
        data: {
            labels: labels,
            datasets: [{
                label: 'Quantidade de Ocorrências',
                data: data,
                backgroundColor: 'rgba(40, 167, 69, 0.5)',
                borderColor: 'rgba(40, 167, 69, 1)',
                borderWidth: 1,
                fill: type === 'line' ? false : true,
                tension: 0.3
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    display: true
                },
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
