window.renderizarGraficoCustoPorTipoDeMaquina = (labels, data, type) => {
    const ctx = document.getElementById('typeMachineCostChart').getContext('2d');

    if (window.typeMachineCostChart instanceof Chart) {
        window.typeMachineCostChart.destroy();
    }

    window.typeMachineCostChart = new Chart(ctx, {
        type: type,
        data: {
            labels: labels,
            datasets: [{
                label: 'Custo por Tipo de Máquina (R$)',
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
                    intersect: false,
                    callbacks: {
                        label: function (ctx) {
                            return `R$ ${ctx.parsed.y.toFixed(2)}`;
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
                        text: 'R$'
                    },
                    ticks: {
                        callback: function (value) {
                            return 'R$ ' + value.toFixed(2);
                        }
                    }
                }
            }
        }
    });
};
