window.renderizarGraficoCustoPorTipoDeProcesso = (labels, data, type) => {
    const ctx = document.getElementById('costTypeProcessChart').getContext('2d');

    if (window.costTypeProcessChart instanceof Chart) {
        window.costTypeProcessChart.destroy();
    }

    window.costTypeProcessChart = new Chart(ctx, {
        type: type,
        data: {
            labels: labels,
            datasets: [{
                label: 'Custo por Tipo de Processo (R$)',
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
                legend: { display: true },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            const value = context.parsed.y;
                            return `R$ ${value.toFixed(2)}`;
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
                        text: 'Reais (R$)'
                    }
                }
            }
        }
    });
};
