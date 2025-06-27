// 全局图表实例
let heatmapChartInstance = null;

// 初始化图表函数
window.initializeHeatmapChart = function() {
    const ctx = document.getElementById('heatmapChart');
    if (!ctx) {
        console.error("无法找到 heatmapChart 元素");
        return;
    }
    
    try {
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
                    x: {
                        type: 'time',
                        time: {
                            unit: 'day',
                            tooltipFormat: 'yyyy-MM-dd',
                            displayFormats: {
                                day: 'MM-dd',
                                week: 'MMM dd',
                                month: 'yyyy-MM'
                            }
                        },
                        title: {
                            display: true,
                            text: '日期'
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
                            text: '浏览次数'
                        }
                    }
                },
                interaction: {
                    intersect: false,
                    mode: 'nearest'
                }
            }
        });
        console.log("图表初始化成功");
    } catch (error) {
        console.error("图表初始化失败:", error);
    }
};

// 更新图表函数
window.updateHeatmapChart = function(type, timeAxis, datasets, yAxisTitle, timeUnit) {
    if (!heatmapChartInstance) {
        console.error("图表实例未初始化");
        return;
    }
    
    try {
        // 销毁旧图表实例
        heatmapChartInstance.destroy();
        
        const ctx = document.getElementById('heatmapChart').getContext('2d');
        
        // 创建新的图表实例
        heatmapChartInstance = new Chart(ctx, {
            type: type,
            data: {
                labels: timeAxis, // 这里确保 timeAxis 是 Date 对象的数组
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
                    x: {
                        type: 'time',
                        time: {
                            unit: timeUnit,
                            tooltipFormat: 'yyyy-MM-dd',
                            displayFormats: {
                                day: 'MM-dd',
                                week: 'MMM dd',
                                month: 'yyyy-MM'
                            }
                        },
                        title: {
                            display: true,
                            text: '日期'
                        },
                        ticks: {
                            maxRotation: 45,
                            minRotation: 45,
                            autoSkip: true,
                            maxTicksLimit: 20
                        }
                    },
                    y: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: yAxisTitle
                        }
                    }
                },
                interaction: {
                    intersect: false,
                    mode: 'nearest'
                }
            }
        });
        console.log("图表更新成功");
    } catch (error) {
        console.error("图表更新失败:", error);
    }
};

let trendChartInstance = null;

// 统一初始化/更新函数
window.updateTrendChart = function(type, timeAxis, datasets, yAxisTitle, timeUnit) {
    const ctxElement = document.getElementById('trendChart');
    if (!ctxElement) {
        console.error("无法找到 trendChart 元素");
        return;
    }
    
    const ctx = ctxElement.getContext('2d');
    
    // 销毁旧实例（如果存在）
    if (trendChartInstance) {
        trendChartInstance.destroy();
        trendChartInstance = null;
    }
    
    try {
        trendChartInstance = new Chart(ctx, {
            type: type,
            data: {
                labels: timeAxis,
                datasets: datasets
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    title: {
                        display: true,
                        text: '用户日常趋势',
                        font: { size: 18 }
                    },
                    tooltip: {
                        mode: 'index',
                        intersect: false,
                        callbacks: {
                            label: function(context) {
                                let label = context.dataset.label || '';
                                if (label) label += ': ';
                                if (context.parsed.y !== null) {
                                    label += context.parsed.y.toLocaleString();
                                }
                                return label;
                            }
                        }
                    },
                    legend: { position: 'top' }
                },
                scales: {
                    x: {
                        type: 'time',
                        time: {
                            unit: timeUnit,
                            tooltipFormat: 'yyyy-MM-dd',
                            displayFormats: {
                                day: 'MM-dd',
                                week: 'MMM dd',
                                month: 'yyyy-MM'
                            }
                        },
                        title: { display: true, text: '日期' },
                        ticks: { 
                            maxRotation: 45, 
                            minRotation: 45, 
                            autoSkip: true, 
                            maxTicksLimit: 20 
                        }
                    },
                    y: {
                        beginAtZero: true,
                        title: { 
                            display: true, 
                            text: yAxisTitle 
                        }
                    }
                },
                interaction: { 
                    intersect: false, 
                    mode: 'nearest' 
                }
            }
        });
        console.log("趋势图表渲染成功");
    } catch (error) {
        console.error("趋势图表渲染失败:", error);
    }
};