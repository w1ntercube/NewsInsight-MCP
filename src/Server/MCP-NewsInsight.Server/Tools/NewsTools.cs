using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MCPNewsInsight.Server.Tools
{
    [McpServerToolType]
    public static class NewsTools
    {
        // MCP 工具：模拟新闻查询
        [McpServerTool, Description("Fetches news headlines.")]
        public static List<string> GetNewsHeadlines()
        {
            // 模拟从数据库中获取新闻头条
            return new List<string>
            {
                "Breaking News: Local Events in Your Area",
                "News Insight: Understanding Technology Trends",
                "Global Updates: The Future of AI"
            };
        }

        // MCP 工具：模拟新闻内容
        [McpServerTool, Description("Fetches news content by headline.")]
        public static string GetNewsContent(string headline)
        {
            return $"Detailed content for {headline}.";
        }
    }
}
