using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModelContextProtocol.Client;
using Anthropic.SDK;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.AI;

class Program
{
    static async Task Main(string[] args)
    {
        // 配置MCP传输
        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "NewsInsight",
            Command = "dotnet",
            Arguments = new[]
            {
                "run",
                "--project",
                @"D:\Projects\DOTNET\NewsInsight\src\Server\MCP-NewsInsight.Server\MCP-NewsInsight.Server.csproj"
            }
        });

        try
        {
            // 创建MCP客户端
            var mcpClient = await McpClientFactory.CreateAsync(transport);

            // 获取所有可用工具
            var tools = await mcpClient.ListToolsAsync();
            Console.WriteLine("可用工具:");
            foreach (var tool in tools)
            {
                Console.WriteLine($"- {tool.Name}: {tool.Description}");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"系统错误: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"技术细节: {ex.InnerException.Message}");
            }
        }
    }
}