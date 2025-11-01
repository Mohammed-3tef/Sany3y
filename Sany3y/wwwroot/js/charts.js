/**
 * Build Chart.js chart of any type from API response.
 *
 * @param {Object} options - configuration object.
 * @param {string} options.type - chart type (ex: 'bar', 'line', 'pie').
 * @param {string} options.url - API url to fetch data from.
 * @param {string} options.canvasId - canvas element id to render chart.
 * @param {string} options.label - dataset label to appear in chart legend.
 * @param {string} options.labelProp - property name used for labels (ex: 'Month').
 * @param {string} options.dataProp - property name used for data values (ex: 'UserCount').
 * 
 * @returns {Promise<Chart>} chart instance (Chart.js object)
 */
function buildChart({ type, url, canvasId, label, labelProp, dataProp }) {
    return new Promise(resolve => {
        $.ajax({
            url: url,
            type: 'GET',
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                console.log("API Response:", data);

                let labels = data.map(x => x[labelProp]);
                let values = data.map(x => x[dataProp]);

                let ctx = document.getElementById(canvasId).getContext('2d');

                let chart = new Chart(ctx, {
                    type: type,
                    data: {
                        labels: labels,
                        datasets: [{
                            label: label,
                            data: values
                        }]
                    }
                });

                resolve(chart);
            }
        });
    });
}

/**
 * Download chart image (PNG format) to user device.
 *
 * @param {Chart} chart - Chart.js instance to convert to image.
 * @param {string} fileNamePrefix - file name prefix (ex: 'Monthly Users').
 */
function downloadChart(chart, fileNamePrefix) {
    const link = document.createElement('a');
    link.download = `${fileNamePrefix}.png`;
    link.href = chart.toBase64Image();
    link.click();
}
