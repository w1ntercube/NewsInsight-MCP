using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models; // 用于Swagger
using NewsInsight.Api.Data;
using NewsInsight.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 连接数据库
builder.Services.AddDbContext<NewsDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("NewsDbConnection"),
        new MySqlServerVersion(new Version(8, 0, 21)))); // MySQL 版本配置

// 添加 Swagger 生成器
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "NewsInsight API", Version = "v1" });
});

// 添加控制器服务
builder.Services.AddControllers();

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

var app = builder.Build();

// 配置 Swagger 和开发环境
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NewsInsight API v1"));
}

// 启用 HTTPS 重定向
app.UseHttpsRedirection();

// 应用 CORS 策略
app.UseCors();  // 这里将启用在上面配置的 CORS 策略

app.MapControllers();

// 添加中间件
app.UseMiddleware<DataRangeMiddleware>();

app.Run();
