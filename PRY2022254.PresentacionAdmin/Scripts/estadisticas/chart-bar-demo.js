// Set new default font family and font color to mimic Bootstrap's default styling

var densityCanvas = document.getElementById("densityChart");

Chart.defaults.global.defaultFontFamily = '-apple-system,system-ui,BlinkMacSystemFont,"Segoe UI",Roboto,"Helvetica Neue",Arial,sans-serif';
Chart.defaults.global.defaultFontColor = '#292b2c';
Chart.defaults.global.defaultFontSize = 18;

var densityData = {
    data: [0, 200, 400, 600, 800, 1000],
    backgroundColor: 'rgba(0, 99, 132, 0.6)',
    
};

var chartOptions = {
    legend: {
        display: false // Establecer la propiedad display en false para ocultar la leyenda
    }
};

var barChart = new Chart(densityCanvas, {
    type: 'bar',
    data: {
        labels: ["Insuficiente", "Iniciado", "Desarrollo", "Definido", "Integrado", "Optimizado",],
        datasets: [densityData]
        
    },
    options: chartOptions
});