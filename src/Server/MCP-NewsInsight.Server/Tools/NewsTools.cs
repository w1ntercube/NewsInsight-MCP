using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MCPNewsInsight.Server.Data;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;

namespace MCPNewsInsight.Server.Tools
{
    [McpServerToolType]
    public class NewsTools
    {
        private readonly NewsDbContext _dbContext;

        // 构造函数，注入 NewsDbContext
        public NewsTools(NewsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [McpServerTool, Description("从数据库获取最新的新闻头条")]
        public async Task<List<string>> GetNewsHeadlines(
            [Description("返回的最大数量")] int maxCount = 5,
            [Description("新闻分类")] string category = null)
        {
            var query = _dbContext.News.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(n => n.Category == category);
            }

            return await query
                .OrderByDescending(n => n.ReleasedTime)
                .Take(maxCount)
                .Select(n => n.Headline)
                .ToListAsync();
        }

        [McpServerTool, Description("根据标题获取新闻完整内容")]
        public async Task<string> GetNewsContent(
            [Description("新闻标题")] string headline)
        {
            var article = await _dbContext.News
                .FirstOrDefaultAsync(n => n.Headline == headline);

            return article != null
                ? $"{article.Headline}\n\n{article.Content}"
                : "未找到匹配的新闻";
        }

        [McpServerTool, Description("获取新闻分类列表")]
        public async Task<List<string>> GetNewsCategories()
        {
            return await _dbContext.News
                .Select(n => n.Category)
                .Distinct()
                .ToListAsync();
        }

        [McpServerTool, Description("获取热门新闻话题")]
        public async Task<List<string>> GetPopularTopics(
            [Description("返回的最大数量")] int maxCount = 5)
        {
            return await _dbContext.News
                .Where(n => !string.IsNullOrEmpty(n.Topic))
                .GroupBy(n => n.Topic)
                .OrderByDescending(g => g.Count())
                .Take(maxCount)
                .Select(g => g.Key)
                .ToListAsync();
        }
    }
}
