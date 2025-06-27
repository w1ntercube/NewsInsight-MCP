using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.Json;
using System;
using System.Text.RegularExpressions;

namespace MCPNewsInsight.Server.Tools
{
    [McpServerToolType]
    public class QueryTools
    {
        private readonly ILogger<QueryTools> _logger;
        private readonly IConfiguration _configuration;

        public QueryTools(
            IConfiguration configuration, 
            ILogger<QueryTools> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // 工具1：由Agent生成SQL语句
        [McpServerTool, Description("由Agent生成安全的SQL查询语句，这个工具return的内容只起到占位的作用")]
        public string GenerateSqlFromQuery(
            [Description("用户自然语言查询请求")] string userQuery)
        {
            // 此方法实际上由Agent实现，这里仅提供占位符
            // Agent会使用内置LLM生成SQL语句
            return "SELECT headline, content, category, topic " +
                   "FROM t_news " +
                   "WHERE MATCH(headline) AGAINST (@query) " +
                   "LIMIT 5";
        }

        // 工具2：执行SQL查询并返回结果
        [McpServerTool, Description("执行Agent生成SQL查询并返回格式化结果")]
        public async Task<string> ExecuteNewsQuery(
            [Description("要执行的SQL查询语句")] string sqlQuery)
        {
            try
            {
                // 验证SQL安全性
                if (!IsSqlSafe(sqlQuery))
                {
                    _logger.LogWarning($"不安全的SQL查询被拒绝: {sqlQuery}");
                    return "错误：查询包含不安全操作";
                }
                
                // 执行SQL查询
                var results = await ExecuteSqlQuery(sqlQuery);
                
                // 格式化为自然语言结果
                return FormatResultsForUser(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQL查询执行失败");
                return $"查询错误: {ex.Message}";
            }
        }

        private bool IsSqlSafe(string sql)
        {
            // 转换为小写以方便检查
            string lowerSql = sql.ToLower().Trim();
            
            // 检查是否以SELECT开头
            if (!lowerSql.StartsWith("select"))
            {
                return false;
            }
            
            // 禁止危险关键词
            string[] forbiddenKeywords = { 
                "insert", "update", "delete", "drop", "alter", 
                "create", "truncate", "grant", "exec", "xp_", 
                ";", "--", "/*", "*/", "union"
            };
            
            foreach (var keyword in forbiddenKeywords)
            {
                if (lowerSql.Contains(keyword))
                {
                    return false;
                }
            }
            
            // 检查是否访问了其他表
            if (Regex.IsMatch(lowerSql, @"\bfrom\s+[^\s]+\b"))
            {
                var matches = Regex.Matches(lowerSql, @"\bfrom\s+([^\s\(\)\,]+)");
                foreach (Match match in matches)
                {
                    string table = match.Groups[1].Value;
                    if (!table.Equals("t_news") && 
                        !table.Equals("`t_news`") &&
                        !table.Equals("'t_news'"))
                    {
                        return false;
                    }
                }
            }
            
            // 检查是否使用了LIMIT
            if (!Regex.IsMatch(lowerSql, @"\blimit\s+\d+", RegexOptions.IgnoreCase))
            {
                // 自动添加LIMIT 10
                sql += " LIMIT 10";
            }
            
            return true;
        }

        private async Task<List<NewsItem>> ExecuteSqlQuery(string sql)
        {
            string connectionString = _configuration["ConnectionStrings:DefaultConnection"] ?? string.Empty;
            var results = new List<NewsItem>();

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            
            using var command = new MySqlCommand(sql, connection);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                results.Add(new NewsItem
                {
                        Headline = reader.GetString(reader.GetOrdinal("headline")),
                        Content = reader.GetString(reader.GetOrdinal("content")),
                        Category = reader.GetString(reader.GetOrdinal("category")),
                        Topic = reader.GetString(reader.GetOrdinal("topic"))
                });
            }
            
            return results;
        }

        private string FormatResultsForUser(List<NewsItem> results)
        {
            if (results.Count == 0)
                return "未找到匹配的新闻";
            
            var response = $"找到 {results.Count} 条相关新闻：\n\n";
            
            foreach (var item in results)
            {
                response += $"**{item.Headline}**\n";
                response += $"分类: {item.Category} | 话题: {item.Topic}\n";
                response += $"{TruncateContent(item.Content ?? string.Empty, 100)}\n\n";
            }
            
            return response;
        }

        private string TruncateContent(string content, int maxLength)
        {
            if (string.IsNullOrEmpty(content))
                return "[内容为空]";
            
            return content.Length <= maxLength 
                ? content 
                : content.Substring(0, maxLength) + "...";
        }
        
        private class NewsItem
        {
            public string? Headline { get; set; }
            public string? Content { get; set; }
            public string? Category { get; set; }
            public string? Topic { get; set; }
        }
    }
}