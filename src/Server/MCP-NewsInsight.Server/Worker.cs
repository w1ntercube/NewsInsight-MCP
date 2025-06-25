using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MCPNewsInsight.Server
{
    public class Worker : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            // 在应用启动时执行的任务
            Console.WriteLine("MCP Server started.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // 在应用停止时执行的任务
            Console.WriteLine("MCP Server stopped.");
            return Task.CompletedTask;
        }
    }
}
