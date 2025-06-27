// 用户兴趣分布饼图模块
let pieChartInstance = null;

// 格式化停留时长（秒）为易读的字符串
function formatDuration(seconds) {
    if (seconds < 60) {
        return seconds + '秒';
    }
    
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    
    if (minutes < 60) {
        return minutes + '分' + (remainingSeconds > 0 ? remainingSeconds + '秒' : '');
    }
    
    const hours = Math.floor(minutes / 60);
    const remainingMinutes = minutes % 60;
    
    return hours + '小时' + 
           (remainingMinutes > 0 ? remainingMinutes + '分' : '') + 
           (remainingSeconds > 0 ? remainingSeconds + '秒' : '');
}

// 初始化饼图函数
window.initializePieChart = function() {
    const ctx = document.getElementById('interestPieChart');
    if (!ctx) {
        console.error("无法找到 interestPieChart 元素");
        return;
    }
    
    try {
        pieChartInstance = new Chart(ctx.getContext('2d'), {
            type: 'pie',
            data: {
                labels: [],
                datasets: []
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    title: {
                        display: true,
                        text: '用户兴趣分布',
                        font: {
                            size: 18
                        },
                        padding: {
                            top: 10,
                            bottom: 20
                        }
                    },
                    subtitle: {
                        display: true,
                        text: '',
                        font: {
                            size: 14,
                            style: 'italic'
                        },
                        padding: {
                            bottom: 20
                        }
                    },
                    legend: {
                        position: 'right',
                        labels: {
                            font: {
                                size: 12
                            },
                            padding: 15,
                            generateLabels: function(chart) {
                                const data = chart.data;
                                if (data.labels.length && data.datasets.length) {
                                    return data.labels.map(function(label, i) {
                                        const meta = chart.getDatasetMeta(0);
                                        const style = meta.controller.getStyle(i);
                                        
                                        return {
                                            text: label,
                                            fillStyle: style.backgroundColor,
                                            strokeStyle: style.borderColor,
                                            lineWidth: style.borderWidth,
                                            hidden: !chart.getDataVisibility(i),
                                            index: i
                                        };
                                    });
                                }
                                return [];
                            }
                        }
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                const label = context.label || '';
                                const value = context.raw || 0;
                                const dataset = context.dataset;
                                const extraData = dataset.extraData[context.dataIndex];
                                
                                // 显示详细数据
                                return [
                                    `类别: ${label}`,
                                    `总点击数: ${extraData.clicks} (${extraData.clicksPercentage}%)`,
                                    `总停留时长: ${extraData.formattedDwellTime} (${extraData.dwellTimePercentage}%)`
                                ];
                            },
                            afterLabel: function(context) {
                                const dataset = context.dataset;
                                const extraData = dataset.extraData[context.dataIndex];
                                
                                // 显示停留时长原始值
                                return `停留时长(秒): ${extraData.dwellTime}`;
                            }
                        }
                    }
                }
            }
        });
        console.log("饼图初始化成功");
    } catch (error) {
        console.error("饼图初始化失败:", error);
    }
};

// 更新饼图函数
window.updatePieChart = function(labels, datasets, title, subtitle) {
    if (!pieChartInstance) {
        console.error("饼图实例未初始化");
        return;
    }
    
    try {
        pieChartInstance.data.labels = labels;
        pieChartInstance.data.datasets = datasets;
        
        // 更新标题和副标题
        pieChartInstance.options.plugins.title.text = title;
        pieChartInstance.options.plugins.subtitle.text = subtitle;
        
        pieChartInstance.update();
        console.log("饼图更新成功");
    } catch (error) {
        console.error("饼图更新失败:", error);
    }
};