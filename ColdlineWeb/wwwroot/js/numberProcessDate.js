window.renderizarGraficoProcess = (labels, data, type) => {
  const ctx = document.getElementById('processChart').getContext('2d');

  // Evita erro caso não seja uma instância válida de Chart
  if (window.processChart instanceof Chart) {
      window.processChart.destroy();
  }

  window.processChart = new Chart(ctx, {
      type: type,
      data: {
          labels: labels,
          datasets: [{
              label: 'Quantidade de Processos',
              data: data,
              backgroundColor: 'rgba(0, 123, 255, 0.5)',
              borderColor: 'rgba(0, 123, 255, 1)',
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
