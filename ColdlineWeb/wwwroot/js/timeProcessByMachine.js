window.renderizarGraficoTempoMaquina = (labels, data, type) => {
    const ctx = document.getElementById('machineProcessChart').getContext('2d');

    if (window.machineProcessChart instanceof Chart) {
        window.machineProcessChart.destroy();
    }

    const maxValue = Math.max(...data);
    const stepSize = Math.ceil(maxValue / 5) || 1;

    window.machineProcessChart = new Chart(ctx, {
        type: type,
        data: {
            labels: labels,
            datasets: [{
                label: 'Tempo Total por Máquina (minutos)',
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
                    intersect: false,
                    callbacks: {
                        label: function (context) {
                            const minutes = context.parsed.y;
                            const hrs = Math.floor(minutes / 60);
                            const min = Math.floor(minutes % 60);
                            return `${hrs}h ${min}min`;
                        }
                    }
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
                    },
                    suggestedMax: maxValue + stepSize,
                    ticks: {
                        stepSize: stepSize,
                        callback: function(value) {
                            const hrs = Math.floor(value / 60);
                            const min = Math.floor(value % 60);
                            return `${hrs}h ${min}min`;
                        }
                    }
                }
            }
        }
    });
};
