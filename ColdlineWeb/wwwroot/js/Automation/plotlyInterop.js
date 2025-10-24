window.plotlyInterop = {
    newPlot: function (elementId, data, layout, config) {
        if (window.Plotly) {
            Plotly.newPlot(elementId, data, layout, config);
        } else {
            console.error("Plotly.js não foi carregado!");
        }
    },
    updatePlot: function (elementId, data, layout) {
        if (window.Plotly) {
            Plotly.react(elementId, data, layout);
        }
    }
};
