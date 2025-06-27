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
using System.Text;

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
        [McpServerTool, Description("1.先将用户的查询请求翻译为英文，再由Agent生成安全的SQL查询语句。 2.这个查询语句不应该包含中文，如果包含，请翻译为英文。 3.语句的SELECT部分必须为eadline, content, category, topic这四部分。 4.这个工具return的内容只起到占位的作用。")]
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
                "--", "union"
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
                    // 添加t_user_interest到白名单
                    if (!table.Equals("t_news") &&
                        !table.Equals("`t_news`") &&
                        !table.Equals("'t_news'") &&
                        !table.Equals("t_user_interest") &&
                        !table.Equals("`t_user_interest`") &&
                        !table.Equals("'t_user_interest'"))
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

        // 新闻辅助类
        private class NewsItem
        {
            public string Headline { get; set; } = string.Empty;
            public string? Content { get; set; }
            public string? Category { get; set; }
            public string? Topic { get; set; }
        }

        // 工具3：根据用户兴趣推荐新闻
        [McpServerTool, Description("根据用户兴趣推荐新闻")]
        public async Task<string> RecommendNewsForUser(
    [Description("要推荐新闻的用户ID")] int userId)
        {
            try
            {
                // 查询用户最感兴趣的类别和话题
                var interest = await GetTopUserInterest(userId);

                List<NewsItem> recommendations;
                string recommendationReason;

                // 如果没有兴趣记录，则随机推荐
                if (interest == null)
                {
                    _logger.LogInformation($"用户 {userId} 无兴趣记录，返回随机推荐");
                    recommendations = await GetRandomRecommendations();
                    recommendationReason = $"用户 {userId} 无历史兴趣记录，以下是随机推荐的新闻：";
                }
                else
                {
                    _logger.LogInformation($"为用户 {userId} 推荐: {interest.Category}/{interest.Topic}");

                    // 根据兴趣类别和话题推荐新闻
                    recommendations = await GetRecommendationsByInterest(
                        interest.Category,
                        interest.Topic
                    );

                    // 如果特定兴趣无结果，放宽到类别推荐
                    if (recommendations.Count == 0)
                    {
                        _logger.LogInformation($"无特定话题新闻，按类别 {interest.Category} 推荐");
                        recommendations = await GetRecommendationsByCategory(interest.Category);
                        recommendationReason = $"用户 {userId} 对 '{interest.Category} > {interest.Topic}' 感兴趣（点击次数: {interest.ClickCount}, 停留: {interest.DwellTime}秒），但未找到相关话题新闻。按类别推荐：";
                    }
                    else
                    {
                        recommendationReason = $"用户 {userId} 对 '{interest.Category} > {interest.Topic}' 感兴趣（点击次数: {interest.ClickCount}, 停留: {interest.DwellTime}秒），推荐新闻：";
                    }
                }

                // 如果特定兴趣无结果，放宽到类别推荐
                if (recommendations.Count == 0)
                {
                    _logger.LogInformation("无任何推荐结果，返回全局随机推荐");
                    recommendations = await GetRandomRecommendations();
                    recommendationReason = $"未找到适合用户 {userId} 的个性化推荐，以下是随机新闻：";
                }

                return FormatRecommendations(recommendationReason, recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"用户 {userId} 推荐失败");
                return $"推荐失败: {ex.Message}";
            }
        }

        private async Task<UserInterest?> GetTopUserInterest(int userId)
        {
            string connectionString = _configuration["ConnectionStrings:DefaultConnection"] ?? string.Empty;

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            // 按点击量和停留时间加权排序
            const string query = @"
        SELECT category, topic, click_count, dwell_time
        FROM t_user_interest
        WHERE user_id = @userId
        ORDER BY (click_count * 0.7 + dwell_time * 0.3) DESC
        LIMIT 1";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new UserInterest
                {
                    Category = reader.GetString(reader.GetOrdinal("category")),
                    Topic = reader.GetString(reader.GetOrdinal("topic")),
                    ClickCount = reader.GetInt32(reader.GetOrdinal("click_count")),
                    DwellTime = reader.GetInt32(reader.GetOrdinal("dwell_time"))
                };
            }

            return null;
        }

        private async Task<List<NewsItem>> GetRecommendationsByInterest(string category, string topic)
        {
            string connectionString = _configuration["ConnectionStrings:DefaultConnection"] ?? string.Empty;

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            // 按兴趣随机推荐
            const string query = @"
        SELECT headline 
        FROM t_news 
        WHERE category = @category AND topic = @topic
        ORDER BY RAND()
        LIMIT 5";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@category", category);
            command.Parameters.AddWithValue("@topic", topic);

            return await ExecuteNewsQuery(command);
        }

        private async Task<List<NewsItem>> GetRecommendationsByCategory(string category)
        {
            string connectionString = _configuration["ConnectionStrings:DefaultConnection"] ?? string.Empty;

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            // 按类别随机推荐
            const string query = @"
        SELECT headline 
        FROM t_news 
        WHERE category = @category
        ORDER BY RAND()
        LIMIT 5";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@category", category);

            return await ExecuteNewsQuery(command);
        }

        private async Task<List<NewsItem>> GetRandomRecommendations()
        {
            string connectionString = _configuration["ConnectionStrings:DefaultConnection"] ?? string.Empty;

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            // 全局随机推荐
            const string query = @"
        SELECT headline 
        FROM t_news 
        ORDER BY RAND()
        LIMIT 5";

            using var command = new MySqlCommand(query, connection);
            return await ExecuteNewsQuery(command);
        }

        private async Task<List<NewsItem>> ExecuteNewsQuery(MySqlCommand command)
        {
            var results = new List<NewsItem>();

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                results.Add(new NewsItem
                {
                    Headline = reader.GetString(reader.GetOrdinal("headline"))
                });
            }

            return results;
        }

        private string FormatRecommendations(string reason, List<NewsItem> recommendations)
        {
            if (recommendations.Count == 0)
            {
                return "未找到任何推荐新闻";
            }

            var response = new StringBuilder();
            response.AppendLine(reason);

            foreach (var item in recommendations)
            {
                response.AppendLine($"- {item.Headline}");
            }

            return response.ToString();
        }

        // 用户兴趣辅助类
        private class UserInterest
        {
            public string Category { get; set; } = string.Empty;
            public string Topic { get; set; } = string.Empty;
            public int ClickCount { get; set; }
            public int DwellTime { get; set; }
        }
    }
}