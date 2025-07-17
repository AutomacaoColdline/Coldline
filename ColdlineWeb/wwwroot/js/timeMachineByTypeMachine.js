window.renderizarGraficoTempoPorTipoDeMaquina = (labels, rawValues, type) => {
    const ctx = document.getElementById('machineTypeChart').getContext('2d');

    if (window.machineTypeChart instanceof Chart) {
        window.machineTypeChart.destroy();
    }

    // Converte "hh:mm:ss" para minutos
    const minutesData = rawValues.map(hms => {
        const parts = hms.split(':').map(Number);
        if (parts.length === 3) {
            return parts[0] * 60 + parts[1] + parts[2] / 60;
        }
        return 0;
    });

    // Função para converter minutos de volta para hh:mm:ss
    function formatMinutesToHMS(minutes) {
        const totalSeconds = Math.round(minutes * 60);
        const h = Math.floor(totalSeconds / 3600);
        const m = Math.floor((totalSeconds % 3600) / 60);
        const s = totalSeconds % 60;
        return `${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
    }

    window.machineTypeChart = new Chart(ctx, {
        type: type,
        data: {
            labels: labels,
            datasets: [{
                label: 'Tempo Médio por Tipo de Máquina',
                data: minutesData,
                backgroundColor: 'rgba(255, 206, 86, 0.5)',
                borderColor: 'rgba(255, 206, 86, 1)',
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
                        label: (context) => {
                            const index = context.dataIndex;
                            const label = labels[index];
                            const originalTime = rawValues[index];
                            return `${label}: ${originalTime}`;
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
