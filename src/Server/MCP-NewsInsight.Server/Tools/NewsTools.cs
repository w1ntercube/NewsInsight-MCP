using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

namespace MCPNewsInsight.Server.Tools
{
    [McpServerToolType]
    public class NewsTools
    {
        private readonly ILogger<NewsTools> _logger;
        private readonly IConfiguration _configuration;

        public NewsTools(IConfiguration configuration, ILogger<NewsTools> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            // 使用直接键访问测试
            var connStr = _configuration["ConnectionStrings:DefaultConnection"];
            _logger.LogInformation("配置注入测试(直接键): {conn}", connStr);
        }

        [McpServerTool, Description("通过标题获取新闻文章")]
        public async Task<string> GetNewsByHeadline(
            [Description("新闻的标题")] string headline)
        {
            // 使用直接键访问连接字符串
            string connectionString = _configuration["ConnectionStrings:DefaultConnection"] ?? string.Empty;
            
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.LogError("MySQL连接字符串未配置");
                
                // 诊断输出所有配置键
                var configKeys = _configuration.AsEnumerable()
                    .Where(kv => !string.IsNullOrEmpty(kv.Value))
                    .Select(kv => kv.Key);
                
                _logger.LogError("非空配置键: {keys}", string.Join(", ", configKeys));
                
                return "数据库配置错误：缺少连接字符串";
            }

            _logger.LogInformation("使用连接字符串: {conn}", connectionString);

            try
            {
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();
                
                using var command = new MySqlCommand(
                    "SELECT content FROM t_news WHERE headline = @headline LIMIT 1", 
                    connection);
                
                command.Parameters.AddWithValue("@headline", headline);

                using var reader = await command.ExecuteReaderAsync();
                
                return await reader.ReadAsync() 
                    ? reader.GetString(reader.GetOrdinal("content"))
                    : "未找到匹配该标题的新闻。";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据库查询失败");
                return $"数据库错误：{ex.Message}";
            }
        }
        
        // 通过新闻ID获取完整新闻内容
        [McpServerTool, Description("通过新闻ID获取完整新闻内容")]
        public async Task<string> GetNewsById(
            [Description("新闻的唯一ID")] int newsId)
        {
            string connectionString = _configuration["ConnectionStrings:DefaultConnection"] ?? string.Empty;
            
            try
            {
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();
                
                var command = new MySqlCommand(
                    "SELECT headline, content, category, topic " +
                    "FROM t_news WHERE news_id = @newsId LIMIT 1", 
                    connection);
                
                command.Parameters.AddWithValue("@newsId", newsId);

                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    // 创建匿名对象包含所有字段
                    var news = new {
                        Headline = reader.GetString(reader.GetOrdinal("headline")),
                        Content = reader.GetString(reader.GetOrdinal("content")),
                        Category = reader.GetString(reader.GetOrdinal("category")),
                        Topic = reader.GetString(reader.GetOrdinal("topic"))
                    };
                    
                    // 序列化为JSON返回
                    return JsonSerializer.Serialize(news);
                }
                
                return "未找到指定ID的新闻";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "新闻查询失败");
                return $"数据库错误：{ex.Message}";
            }
        }

        // 通过关键词全文搜索新闻标题
        [McpServerTool, Description("根据关键词搜索新闻标题，如果关键词不是英文，请先翻译为英文，返回匹配的标题数量由num参数指定，默认为5")]
        public async Task<List<string>> SearchNewsByKeywords(
            [Description("用于搜索的关键词，可以是多个词")] string keywords,
            [Description("指定返回的新闻条目数，默认为5")] int num = 5)
        {
            string connectionString = _configuration["ConnectionStrings:DefaultConnection"] ?? string.Empty;
            var results = new List<string>();

            try
            {
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                // 使用全文索引进行搜索，按相关性排序
                var command = new MySqlCommand(
                    "SELECT headline, MATCH(headline) AGAINST (@keywords) AS relevance " +
                    "FROM t_news " +
                    "WHERE MATCH(headline) AGAINST (@keywords) " +
                    "ORDER BY relevance DESC " +
                    "LIMIT @num", 
                    connection);

                // 添加查询参数
                command.Parameters.AddWithValue("@keywords", keywords);
                command.Parameters.AddWithValue("@num", num);  // 使用用户指定的num作为LIMIT的参数

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    results.Add(reader.GetString(reader.GetOrdinal("headline")));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "新闻搜索失败");
                // 返回错误信息作为第一条结果
                results.Add($"搜索失败：{ex.Message}");
            }

            return results;
        }

        // 获取随机新闻标题
        [McpServerTool, Description("根据分类获取新闻标题")]
        public async Task<List<string>> GetNewsByCategory(
            [Description("新闻分类")] string category,
            [Description("返回数量，默认5条")] int num = 5)
        {
            string connectionString = _configuration["ConnectionStrings:DefaultConnection"] ?? string.Empty;
            var headlines = new List<string>();

            try
            {
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();
                
                // 使用RAND()随机排序并限制返回数量
                var command = new MySqlCommand(
                    "SELECT headline " +
                    "FROM t_news " +
                    "WHERE category = @category " +
                    "ORDER BY RAND() " +  // 随机排序
                    "LIMIT @limit", 
                    connection);
                
                command.Parameters.AddWithValue("@category", category);
                command.Parameters.AddWithValue("@limit", num);

                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    headlines.Add(reader.GetString(reader.GetOrdinal("headline")));
                }
                
                // 如果结果不足请求数量，补充说明
                if (headlines.Count < num)
                {
                    headlines.Add($"（仅找到 {headlines.Count} 条{category}分类新闻）");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "随机新闻获取失败");
                headlines.Add($"错误: {ex.Message}");
            }

            return headlines;
        }
    }
}