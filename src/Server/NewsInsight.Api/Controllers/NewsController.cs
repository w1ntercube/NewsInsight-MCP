using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsInsight.Api.Data;
using NewsInsight.Api.Models;
using System.Linq;
using NewsInsight.Shared.Models.DTOs;
using NewsInsight.Shared.Models.Utils;
using NewsInsight.Api.Services;
using ManagedBridge;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewsInsight.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly NewsDbContext _context;
        private readonly IPrefixMatcherService _prefixMatcherService;
        private readonly ILogger<NewsController> _logger;

        public NewsController(
            NewsDbContext context,
            IPrefixMatcherService prefixMatcherService,
            ILogger<NewsController> logger)
        {
            _context = context;
            _prefixMatcherService = prefixMatcherService;
            _logger = logger;
        }

        // 一次性初始化机制
        private static bool _isInitialized = false;
        private static readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);

        // 初始化逻辑
        private async Task EnsureCategoryMatcherInitializedAsync()
        {
            if (IsMatcherInitialized()) return; // 第一次快速检查

            await _initLock.WaitAsync();
            try
            {
                if (IsMatcherInitialized()) return; // 第二次确认检查

                _logger.LogInformation("开始初始化类别匹配器...");
                var categories = await _context.News
                    .AsNoTracking()
                    .Select(n => n.Category)
                    .Distinct()
                    .ToListAsync();

                _logger.LogInformation($"从数据库获取到 {categories.Count} 个类别");
                _prefixMatcherService.InitializeMatcher(categories);
            }
            finally
            {
                _initLock.Release();
            }
        }

        private bool IsMatcherInitialized()
        {
            return _prefixMatcherService.IsInitialized(); // 正确检查初始化状态
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
            // 使用原始上下文实例
            var query = _context.News.AsNoTracking().AsQueryable();

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
                Content = news.Content
            });
        }

        // 获取所有类别
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<string>>> GetAllCategories()
        {
            var categories = await _context.News
                .AsNoTracking()
                .Select(n => n.Category)
                .Distinct()
                .ToListAsync();
            return Ok(categories);
        }


        // 获取匹配的类别
        [HttpGet("categories/match")]
        public async Task<ActionResult<IEnumerable<string>>> MatchCategories([FromQuery] string prefix)
        {
            
            try
            {
                _logger.LogInformation($"匹配类别请求: 前缀 '{prefix}'");

                // 确保匹配器已初始化
                await EnsureCategoryMatcherInitializedAsync();

                var matched = _prefixMatcherService.MatchPrefix(prefix);
                return Ok(matched);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"匹配类别失败: 前缀 '{prefix}'");
                return StatusCode(500, "内部服务器错误");
            }
        }

        // 缓存刷新
        [HttpPost("categories/refresh")]
        public async Task<IActionResult> RefreshCategoryMatcher()
        {
            try
            {
                // 重新初始化匹配器
                var categories = await _context.News
                    .AsNoTracking()
                    .Select(n => n.Category)
                    .Distinct()
                    .ToListAsync();

                _prefixMatcherService.InitializeMatcher(categories);
                return Ok("Category matcher refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh category matcher");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}