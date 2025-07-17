window.renderizarGraficoTipoProcesso = (labels, data, type) => {
    const ctx = document.getElementById('processTypeChart').getContext('2d');

    if (window.processTypeChart instanceof Chart) {
        window.processTypeChart.destroy();
    }

    const maxValue = Math.max(...data);
    const stepSize = Math.ceil(maxValue / 5) || 1;

    window.processTypeChart = new Chart(ctx, {
        type: type,
        data: {
            labels: labels,
            datasets: [{
                label: 'Tempo de Processo por Tipo (minutos)',
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
                    intersect: false,
                    callbacks: {
                        label: function(context) {
                            const totalMin = context.parsed.y;
                            const hours = Math.floor(totalMin / 60);
                            const minutes = Math.round(totalMin % 60);
                            return `${hours}h ${minutes}min`;
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
