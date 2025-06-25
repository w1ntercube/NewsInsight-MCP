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
        // 创建配置
        var builder = Host.CreateApplicationBuilder(args);
        builder.Configuration
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>();
        var configuration = builder.Configuration;

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

            // 创建Anthropic聊天客户端
            var anthropicClient = new AnthropicClient(
                new APIAuthentication(configuration["ANTHROPIC_API_KEY"]));

            // 构建带工具支持的聊天客户端
            var chatClient = anthropicClient.Messages
                .AsBuilder()
                .UseFunctionInvocation()
                .Build();

            // 配置聊天选项
            var options = new ChatOptions
            {
                MaxOutputTokens = 1000,
                ModelId = "claude-3-haiku-20240307", // 推荐使用更快的模型
                Tools = [.. tools] // 注入所有工具
            };

            Console.WriteLine("新闻分析助手已启动 (输入 'exit' 退出)");

            while (true)
            {
                Console.Write("\n您的问题: ");
                var question = Console.ReadLine();

                if (string.Equals(question, "exit", StringComparison.OrdinalIgnoreCase))
                    break;

                if (string.IsNullOrWhiteSpace(question))
                    continue;

                // 流式获取响应（自动处理工具调用）
                Console.WriteLine("\nAI回复:");
                Console.WriteLine("----------------------------------------");

                await foreach (var message in chatClient.GetStreamingResponseAsync(question, options))
                {
                    Console.Write(message);
                }

                Console.WriteLine("\n----------------------------------------");
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