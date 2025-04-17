window.renderizarGraficoUsuarioIndividual = (labels, data, usuario, type) => {
    const ctx = document.getElementById('userIndividualChart').getContext('2d');

    if (window.userIndividualChart instanceof Chart) {
        window.userIndividualChart.destroy();
    }

    window.userIndividualChart = new Chart(ctx, {
        type: type,
        data: {
            labels: labels,
            datasets: [{
                label: `Tempo de Processo de ${usuario} por Tipo (min)`,
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
                        text: 'Minutos'
                    }
                }
            }
        }
    });
};
