// 确保在全局作用域中定义 heatmapChartInstance
let heatmapChartInstance = null;

// 初始化图表函数
window.initializeHeatmapChart = function () {
    console.log("Initializing heatmap chart...");
    const ctx = document.getElementById('heatmapChart');
    if (!ctx) return;
    
    heatmapChartInstance = new Chart(ctx.getContext('2d'), {
        type: 'line',
        data: {
            labels: [],
            datasets: []
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                title: {
                    display: true,
                    text: '类别热度趋势图',
                    font: {
                        size: 18
                    }
                },
                tooltip: {
                    mode: 'index',
                    intersect: false
                },
                legend: {
                    position: 'top',
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: '浏览次数'
                    }
                },
                x: {
                    title: {
                        display: true,
                        text: '日期'
                    }
                }
            },
            interaction: {
                intersect: false,
                mode: 'nearest'
            }
        }
    });
};

// 更新图表函数
window.updateHeatmapChart = function(type, labels, datasets, yAxisTitle) {
    if (!heatmapChartInstance) return;
    
    // 销毁旧图表实例
    heatmapChartInstance.destroy();
    
    const ctx = document.getElementById('heatmapChart').getContext('2d');
    heatmapChartInstance = new Chart(ctx, {
        type: type,
        data: {
            labels: labels,
            datasets: datasets
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                title: {
                    display: true,
                    text: '类别热度趋势图',
                    font: {
                        size: 18
                    }
                },
                tooltip: {
                    mode: 'index',
                    intersect: false,
                    callbacks: {
                        label: function(context) {
                            let label = context.dataset.label || '';
                            if (label) {
                                label += ': ';
                            }
                            if (context.parsed.y !== null) {
                                label += context.parsed.y.toLocaleString();
                            }
                            return label;
                        }
                    }
                },
                legend: {
                    position: 'top',
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: yAxisTitle
                    }
                },
                x: {
                    title: {
                        display: true,
                        text: '日期'
                    }
                }
            },
            interaction: {
                intersect: false,
                mode: 'nearest'
            }
        }
    });
};