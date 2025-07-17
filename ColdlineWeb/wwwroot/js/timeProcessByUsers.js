window.renderizarGraficoTempoUsuarios = (labels, data, type, rawTimes) => {
    const ctx = document.getElementById('timeChart').getContext('2d');

    if (window.timeChart instanceof Chart) {
        window.timeChart.destroy();
    }

    function formatMinutesToHMS(minutes) {
        const totalSeconds = Math.round(minutes * 60);
        const h = Math.floor(totalSeconds / 3600);
        const m = Math.floor((totalSeconds % 3600) / 60);
        const s = totalSeconds % 60;
        return `${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
    }

    window.timeChart = new Chart(ctx, {
        type: type,
        data: {
            labels: labels,
            datasets: [{
                label: 'Tempo de Processo por Usuário',
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
                    callbacks: {
                        label: function (context) {
                            const i = context.dataIndex;
                            return `${labels[i]}: ${rawTimes[i]}`;
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
                        text: 'Tempo (hh:mm:ss)'
                    },
                    ticks: {
                        callback: function (value) {
                            return formatMinutesToHMS(value);
                        }
                    }
                }
            }
        }
    });
};
