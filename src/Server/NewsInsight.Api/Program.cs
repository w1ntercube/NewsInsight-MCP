using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models; // 用于Swagger
using NewsInsight.Api.Data;
using NewsInsight.Api.Middleware;
using NewsInsight.Api.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5002); // 监听所有IP的5002端口（HTTP）
});

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    // 获取当前应用程序基目录
    string basePath = AppContext.BaseDirectory;
    
    // 添加 lib 目录到 DLL 搜索路径
    string libPath = Path.Combine(basePath, "lib");
    
    // 使用 SetDllDirectory 添加路径
    SetDllDirectory(libPath);
}

// 导入 SetDllDirectory 函数
[DllImport("kernel32.dll", SetLastError = true)]
static extern bool SetDllDirectory(string lpPathName);


// 连接数据库
builder.Services.AddDbContextPool<NewsDbContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("NewsDbConnection"),
        new MySqlServerVersion(new Version(8, 0, 21)),
        optionsBuilder =>
        {
            optionsBuilder.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });
});

// 添加 Swagger 生成器
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "NewsInsight API", Version = "v1" });
});

// 添加控制器服务
builder.Services.AddControllers();

// 添加服务注册
builder.Services.AddSingleton<IPrefixMatcherService, PrefixMatcherService>();

builder.Logging.AddConsole();
builder.Logging.AddDebug();

// 配置 CORS（允许特定来源）
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:5245") // 允许来自前端的 http://localhost:5245 请求
               .AllowAnyHeader()  // 允许所有请求头
               .AllowAnyMethod(); // 允许所有方法（GET, POST, PUT, DELETE 等）
    });
});

// 添加健康检查
builder.Services.AddHealthChecks()
    .AddMySql(
        builder.Configuration.GetConnectionString("NewsDbConnection") ?? throw new InvalidOperationException("Connection string 'NewsDbConnection' is missing."),
        name: "mysql",
        failureStatus: HealthStatus.Degraded);

var app = builder.Build();

// 添加健康检查端点
app.MapHealthChecks("/health");

// 配置 Swagger 和开发环境
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NewsInsight API v1"));


// 不启用 HTTPS 重定向
// app.UseHttpsRedirection();

// 应用 CORS 策略
app.UseCors();  // 这里将启用在上面配置的 CORS 策略

app.MapControllers();

// 添加中间件
app.UseMiddleware<DataRangeMiddleware>();

app.Environment.EnvironmentName = "Production";

app.Run();
