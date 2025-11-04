window.reportCharts = {
  charts: {},

  generate(canvasId, type, labels, datasets) {
    console.groupCollapsed(`[reportCharts] Gerando gráfico: ${canvasId}`);
    console.time(`RenderTime_${canvasId}`);

    try {
      console.log("📊 Dados recebidos:", { canvasId, type, labels, datasets });

      const canvas = document.getElementById(canvasId);
      if (!canvas) {
        console.warn(`⚠️ Canvas '${canvasId}' não encontrado no DOM.`);
        console.groupEnd();
        return;
      }

      if (this.charts[canvasId]) {
        console.log("🧹 Limpando gráfico anterior...");
        this.charts[canvasId].destroy();
      }

      if (!Array.isArray(labels) || !Array.isArray(datasets)) {
        console.error(`❌ Dados inválidos para ${canvasId}`, { labels, datasets });
        console.groupEnd();
        return;
      }

      const ctx = canvas.getContext("2d");
      const chartType = (type || "line").toLowerCase();

      const chartData = {
        labels,
        datasets: datasets.map((d, i) => ({
          label: d.label || `Série ${i + 1}`,
          data: d.data || [],
          borderWidth: 2,
          tension: 0.3,
          fill: chartType !== "line",
          backgroundColor: this.color(i, 0.4),
          borderColor: this.color(i, 0.9),
          pointRadius: chartType === "line" ? 3 : 0,
          pointHoverRadius: chartType === "line" ? 6 : 0
        }))
      };

      const options = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: "bottom" },
          tooltip: { enabled: true }
        },
        scales:
          chartType === "doughnut" || chartType === "pie"
            ? undefined
            : {
                x: { grid: { color: "rgba(0,0,0,0.05)" } },
                y: {
                  beginAtZero: true,
                  grid: { color: "rgba(0,0,0,0.05)" }
                }
              }
      };

      this.charts[canvasId] = new Chart(ctx, { type: chartType, data: chartData, options });

      console.log(`✅ Gráfico '${canvasId}' renderizado (${chartType})`);
    } catch (err) {
      console.error(`❌ Erro ao gerar gráfico '${canvasId}':`, err);
    } finally {
      console.timeEnd(`RenderTime_${canvasId}`);
      console.groupEnd();
    }
  },

  color(i, opacity = 1) {
    const palette = [
      [13, 110, 253],
      [25, 135, 84],
      [255, 193, 7],
      [220, 53, 69],
      [102, 16, 242]
    ];
    const [r, g, b] = palette[i % palette.length];
    return `rgba(${r},${g},${b},${opacity})`;
  }
};
