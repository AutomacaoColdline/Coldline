window.renderizarGraficoUsuarioIndividual = (labels, data, usuario, type) => {
    const ctx = document.getElementById('userIndividualChart').getContext('2d');

    if (window.userIndividualChart instanceof Chart) {
        window.userIndividualChart.destroy();
    }

    const maxValue = Math.max(...data);
    const stepSize = Math.ceil(maxValue / 5) || 1;

    window.userIndividualChart = new Chart(ctx, {
        type: type,
        data: {
            labels: labels,
            datasets: [{
                label: `Tempo de Processo de ${usuario} por Tipo (min)`, // continua em minutos
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
                            const min = Math.round(minutes % 60);
                            return `${hrs}h ${min}min`; // permanece assim
                        }
                    }
                }
            },
            scales: {
                x: {
                    title: {
                        display: true,
                        text: 'Tipo de Processo'
                    },
                    ticks: {
                        maxRotation: 45,
                        minRotation: 45
                    }
                },
                y: {
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Horas' // ✅ muda aqui apenas
                    },
                    suggestedMax: maxValue + stepSize,
                    ticks: {
                        stepSize: stepSize,
                        callback: function (value) {
                            const h = Math.floor(value / 60);
                            return `${h}h`; // ✅ mostra apenas hora inteira
                        }
                    }
                }
            }
        }
    });
};
