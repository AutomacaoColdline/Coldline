window.renderizarGraficoTempoPorTipoDeProcesso = (labels, data, type) => {
    const ctx = document.getElementById('typeProcessChart').getContext('2d');

    if (window.typeProcessChart instanceof Chart) {
        window.typeProcessChart.destroy();
    }

    window.typeProcessChart = new Chart(ctx, {
        type: type,
        data: {
            labels: labels,
            datasets: [{
                label: 'Tempo de Processo por Tipo (minutos)',
                data: data,
                backgroundColor: 'rgba(54, 162, 235, 0.5)',
                borderColor: 'rgba(54, 162, 235, 1)',
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
