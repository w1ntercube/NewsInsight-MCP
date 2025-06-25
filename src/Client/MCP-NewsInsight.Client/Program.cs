using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ModelContextProtocol.Client;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // 配置传输
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

            // 创建客户端
            var client = await McpClientFactory.CreateAsync(transport);
            Console.WriteLine("Connected to MCP server");

            // 打印 MCP 服务器提供的所有工具列表
            var tools = await client.ListToolsAsync();
            Console.WriteLine("Available tools:");
            foreach (var tool in tools)
            {
                Console.WriteLine($"{tool.Name} ({tool.Description})");
            }
            // 使用客户端...
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine("MCP Client initialization timed out.");
            Console.WriteLine($"Error details: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine("The task was canceled.");
            Console.WriteLine($"Error details: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}