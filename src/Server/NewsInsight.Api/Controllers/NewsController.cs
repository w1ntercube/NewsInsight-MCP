using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsInsight.Api.Data;
using NewsInsight.Api.Models;
using System.Linq;
using NewsInsight.Shared.Models.DTOs;
using NewsInsight.Shared.Models.Utils;

namespace NewsInsight.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly NewsDbContext _context;

        public NewsController(NewsDbContext context)
        {
            _context = context;
        }

        // 分页获取新闻列表（添加过滤和排序）
        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<NewsDto>>> GetNews(
            [FromQuery] string? category = null,
            [FromQuery] string? keyword = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string sortBy = "released",
            [FromQuery] bool sortDesc = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _context.News.AsQueryable();

            // 添加过滤条件
            if (!string.IsNullOrEmpty(category))
                query = query.Where(n => n.Category == category);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(n => n.Headline.Contains(keyword) || n.Content.Contains(keyword));

            if (startDate.HasValue)
            {
                var startTime = DateTimeUtils.ConvertToUnixTimestamp(startDate.Value);
                query = query.Where(n => n.ReleasedTime >= startTime);
            }

            if (endDate.HasValue)
            {
                var endTime = DateTimeUtils.ConvertToUnixTimestamp(endDate.Value);
                query = query.Where(n => n.ReleasedTime <= endTime);
            }

            // 添加排序
            query = sortBy.ToLower() switch
            {
                "popularity" => sortDesc ?
                    query.OrderByDescending(n => n.TotalBrowseNum) :
                    query.OrderBy(n => n.TotalBrowseNum),
                "duration" => sortDesc ?
                    query.OrderByDescending(n => n.TotalBrowseDuration) :
                    query.OrderBy(n => n.TotalBrowseDuration),
                _ => sortDesc ?
                    query.OrderByDescending(n => n.ReleasedTime) :
                    query.OrderBy(n => n.ReleasedTime)
            };

            var totalCount = await query.CountAsync();
            var newsList = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NewsDto
                {
                    Id = n.NewsId,
                    Headline = n.Headline,
                    Category = n.Category,
                    Topic = n.Topic,
                    ReleasedTime = DateTimeUtils.ConvertFromUnixTimestamp(n.ReleasedTime),
                    BrowseCount = n.TotalBrowseNum
                })
                .ToListAsync();

            return Ok(new PaginatedResponse<NewsDto>(newsList, page, pageSize, totalCount));
        }

        // 获取单条新闻
        [HttpGet("{id}")]
        public async Task<ActionResult<NewsDto>> GetNewsById(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null)
            {
                return NotFound();
            }

            return Ok(new NewsDto
            {
                Id = news.NewsId,
                Headline = news.Headline,
                Category = news.Category,
                Topic = news.Topic,
                ReleasedTime = DateTimeUtils.ConvertFromUnixTimestamp(news.ReleasedTime),
                BrowseCount = news.TotalBrowseNum,
                Content = news.Content // 确保返回 Content 字段
            });
        }
    }
}
