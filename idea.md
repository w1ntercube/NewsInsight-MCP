
### **NewsInsight 项目架构概览**

这个项目的目标是构建一个 **新闻智能分析平台**，其核心部分包括 **C# 后端 API、C++/CLI 与 C++ 的集成**、以及 **Blazor 前端**。同时，后端会与 **外部 LLM API** 集成，通过自然语言处理技术（如情感分析、关键词提取等）来处理新闻数据。项目可能会部署到 **IIS** 上并支持 **HTTPS**。

### **项目模块与技术栈**

#### **1. 前端模块（Blazor WebAssembly）**

* **技术**：`Blazor WebAssembly`、`C#`、`HTML`、`CSS`
* **功能**：

  * 提供用户界面（UI）以展示新闻内容和分析结果。
  * 用户可以输入新闻内容并提交，前端通过 **`HttpClient`** 发送请求到后端。
  * 显示分析结果（例如情感分析、关键词提取等）。


#### **2. 后端模块（ASP.NET Core API）**

* **技术**：`ASP.NET Core 6`、`C#`、`HttpClient`、`Entity Framework Core`（未来可能用于数据库访问）、`IConfiguration`（用于存储 API 密钥）
* **功能**：

  * 提供 API 端点，接收来自 Blazor 前端的请求。
  * 代理请求到外部 LLM API（如 Doubao LLM API）进行情感分析、关键词提取等。
  * 处理和返回外部 API 的响应结果。
  * 处理 **API 密钥** 和 **认证**，确保安全性。
  * **CORS** 配置，允许来自 Blazor 前端的跨域请求。

#### **3. 外部 LLM API 集成（如 Doubao LLM API）**

* **技术**：`HTTP` 请求（通过 `HttpClient`）
* **功能**：

  * 后端通过 `HttpClient` 将请求发送到外部 LLM API（如 Doubao LLM API）。
  * 获取文本分析结果并返回给前端。
  * 支持情感分析、关键词提取、文本分类等自然语言处理功能。


#### **4. 数据库模块（未来扩展）**

* **技术**：`MySQL`、`Entity Framework Core`、`C#`
* **功能**：

  * 存储用户输入的新闻数据和分析结果。
  * 支持查询历史数据，生成报告等。
  * 后端 API 可以与数据库进行交互，保存和检索数据。


#### **5. C++/CLI 与 C++ 模块（Win32 DLL）**

* **技术**：`C++`、`C++/CLI`、`Win32 API`、`PInvoke`
* **功能**：

  * **C++/CLI**：提供一个桥接模块，允许 .NET（C#）代码调用 **C++** 编写的低层次函数。
  * **Win32 DLL**：提供一些性能敏感的操作（如文本分析、数据处理等），并通过 Win32 DLL 导出接口给 C# 调用。
  * 这些模块可能包含一些复杂的算法，帮助优化性能和提供更强大的计算能力。


#### **6. 部署与环境配置（IIS 部署、SSL 配置）**

* **技术**：`IIS`、`HTTPS`、`Docker`（可能），`CI/CD`
* **功能**：

  * 配置 **IIS** 部署前端和后端。
  * 配置 **SSL 证书**，启用 **HTTPS**，确保在生产环境中的安全性。
  * 配置自动化部署（如通过 **CI/CD** 管道），确保开发、测试、生产环境的一致性。




NewsInsight/
├── src/
│   ├── Client/
│   │   ├── MCPBlazorApp/             # Blazor WebAssembly 前端
│   │   │   ├── Pages/
│   │   │   ├── wwwroot/
│   │   │   ├── Program.cs
│   │   │   └── MCPBlazorApp.csproj
│   ├── Server/
│   │   ├── NewsInsight.Api/          # 业务API层
│   │   │   ├── Controllers/
│   │   │   ├── Services/             # 业务逻辑服务
│   │   │   ├── Data/
│   │   │   │   └── NewsDbContext.cs  # 数据库上下文
│   │   │   ├── Lib/
│   │   │   │   └── NativePrefixMatcher.dll
│   │   │   │   └── PrefixMatcherWrapper.dll
│   │   │   ├── Middleware/
│   │   │   ├── Properties/
│   │   │   ├── appsettings.json
│   │   │   ├── Program.cs
│   │   │   └── NewsInsight.Api.csproj
│   │   └── MCP-NewsInsight.Server/   # MCP服务层
│   │       ├── Tools/                # MCP工具实现
│   │       │   └── NewsTools.cs
│   │       ├── Data
│   │       ├── appsettings.json
│   │       ├── Program.cs
│   │       └── MCP-NewsInsight.Server.csproj
│   └── Shared/
│       └── NewsInsight.Shared.Models/ # 共享模型
│           ├── Entities/             # 数据库实体
│           │   ├── News.cs
│           │   ├── NewsBrowseRecord.cs
│           │   ├── NewsCategory.cs
│           │   └── UserInterest.cs
│           ├── DTOs/                 # 数据传输对象
│           ├── Utils/                # 工具类
│           └── NewsInsight.Shared.Models.csproj
├── docs/                             # 文档
└── NewsInsight.sln                   # 解决方案文件


MCP 服务与 LLM 集成方案
MCP（Model Context Protocol）在此架构中充当了客户端与服务器之间的通信协议。它通过工具（如 NewsTools）提供具体的功能（例如获取新闻头条、新闻内容）。为将 LLM 集成到 MCP 服务中，我们可以采取以下步骤：

1. 定义一个 MCP 工具来调用 LLM API
您可以创建一个新的 MCP 工具，命名为 LLMTools 或类似名称。

这个工具将与 LLM 后端服务通信，通过调用 LLM API 来获取新闻分析、摘要、情感分析等功能。

2. LLM 集成流程
用户从客户端传递请求，MCP 服务器会接收请求。

服务器根据请求调用与 LLM API 集成的工具来处理数据，可能包括新闻内容的分析、情感分析或生成新闻。

然后，MCP 服务器会将处理后的结果返回给客户端。



## 部署相关


### 发布

对三个项目都使用：
``` bash
dotnet publish -c Release -o ./publish
```

### 前端
``` bash
sudo vim /etc/nginx/nginx.conf
....
    server {
        listen       80;
        listen       [::]:80;
        server_name  14.103.142.209;
        root         /var/www/NewsInsight;

        location / 
        {
                index index.html;
                }

        location /_framework/
        {
                root /var/www/NewsInsight;
                }

        location /css/ 
        {
                root /var/www/NewsInsight;
                }

        location /js/ 
        {
                root /var/www/NewsInsight;
                } 

        location /lib/
        { 
                root /var/www/NewsInsight;
                }


        # Load configuration files for the default server block.
        include /etc/nginx/default.d/*.conf;

        error_page 404 /404.html;
        location = /404.html 
        {
        }

        error_page 500 502 503 504 /50x.html;
        location = /50x.html 
        {
        }

cd ./publish/wwwroot
dotnet --server.urls=http://localhost:5245
```