window.renderizarGraficoBrokenParts = (labels, data, type) => {
    const ctx = document.getElementById('brokenPartChart').getContext('2d');

    if (window.brokenPartChart instanceof Chart) {
        window.brokenPartChart.destroy();
    }

    window.brokenPartChart = new Chart(ctx, {
        type: type,
        data: {
            labels: labels,
            datasets: [{
                label: 'Peças Quebradas',
                data: data,
                backgroundColor: 'rgba(220, 53, 69, 0.5)',
                borderColor: 'rgba(220, 53, 69, 1)',
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
