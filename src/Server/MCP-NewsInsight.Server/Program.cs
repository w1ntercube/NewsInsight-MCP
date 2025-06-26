using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Microsoft.Extensions.Configuration;
using System.IO;

// 获取应用程序基目录
var basePath = AppContext.BaseDirectory;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    ContentRootPath = basePath, // 明确设置内容根路径
    ApplicationName = "MCP-NewsInsight.Server"
});

var appSettingsPath = Path.Combine(basePath, "appsettings.json");
Console.WriteLine($"加载配置文件: {appSettingsPath}");
Console.WriteLine($"文件存在: {File.Exists(appSettingsPath)}");

builder.Configuration
    .AddJsonFile(appSettingsPath, optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// 验证配置加载
var config = builder.Configuration;
Console.WriteLine($"配置加载测试: {config.GetConnectionString("DefaultConnection")}");

// 配置日志输出
builder.Logging.AddConsole(consoleLogOptions => {
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

// 注册 MCP 服务器并添加工具
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();