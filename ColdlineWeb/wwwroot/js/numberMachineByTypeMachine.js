window.renderizarGraficoTipoMaquina = (labels, data, type) => {
    const ctx = document.getElementById('machineTypeChart').getContext('2d');

    if (window.machineTypeChart instanceof Chart) {
        window.machineTypeChart.destroy();
    }

    const maxValue = Math.max(...data);
    const stepSize = Math.ceil(maxValue / 5) || 1;

    window.machineTypeChart = new Chart(ctx, {
        type: type,
        data: {
            labels: labels,
            datasets: [{
                label: 'Máquinas por Tipo de Máquina',
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
                    suggestedMax: maxValue + stepSize,
                    ticks: {
                        stepSize: stepSize
                    },
                    title: {
                        display: true,
                        text: 'Quantidade'
                    }
                }
            }
        }
    });
};
