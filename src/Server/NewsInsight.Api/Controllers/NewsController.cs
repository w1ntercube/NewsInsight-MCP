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
        private async Task EnsureMatchersInitializedAsync()
        {
            if (IsMatcherInitialized())
                return;

            _logger.LogInformation("开始初始化匹配器...");
            await _initLock.WaitAsync();
            try
            {
                if (IsMatcherInitialized())
                    return;

                // 顺序执行查询
                var categories = await _context.News
                    .AsNoTracking()
                    .Select(n => n.Category)
                    .Distinct()
                    .ToListAsync();

                var topics = await _context.News
                    .AsNoTracking()
                    .Select(n => n.Topic)
                    .Distinct()
                    .ToListAsync();

                _logger.LogInformation($"从数据库获取到 {categories.Count} 个类别和 {topics.Count} 个主题");

                // 初始化两个匹配器
                _prefixMatcherService.InitializeCategoryMatcher(categories);
                _prefixMatcherService.InitializeTopicMatcher(topics);

                _logger.LogInformation("匹配器初始化完成");
            }
            finally
            {
                _initLock.Release();
            }
        }

        private bool IsMatcherInitialized()
        {
            // 检查两个匹配器是否都已初始化
            return _prefixMatcherService.IsCategoryInitialized() &&
                   _prefixMatcherService.IsTopicInitialized();
        }
        // 分页获取新闻列表（添加过滤和排序）
        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<NewsDto>>> GetNews(
            [FromQuery] string? category = null,
            [FromQuery] string? topic = null,
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

            if (!string.IsNullOrEmpty(topic))
                query = query.Where(n => n.Topic == topic);

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

        // 缓存刷新
        [HttpPost("matchers/refresh")]
        public async Task<IActionResult> RefreshMatchers()
        {
            try
            {
                // 重新获取类别和主题
                var categories = await _context.News
                    .AsNoTracking()
                    .Select(n => n.Category)
                    .Distinct()
                    .ToListAsync();

                var topics = await _context.News
                    .AsNoTracking()
                    .Select(n => n.Topic)
                    .Distinct()
                    .ToListAsync();

                // 刷新两个匹配器
                _prefixMatcherService.InitializeCategoryMatcher(categories);
                _prefixMatcherService.InitializeTopicMatcher(topics);

                return Ok("匹配器刷新成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刷新匹配器失败");
                return StatusCode(500, "内部服务器错误");
            }
        }

        // 获取匹配的类别
        [HttpGet("categories/match")]
        public async Task<ActionResult<IEnumerable<string>>> MatchCategories([FromQuery] string prefix)
        {
            try
            {
                _logger.LogInformation($"匹配类别请求: 前缀 '{prefix}'");

                // 确保匹配器已初始化
                await EnsureMatchersInitializedAsync();

                var matched = _prefixMatcherService.MatchCategoryPrefix(prefix);
                return Ok(matched);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"匹配类别失败: 前缀 '{prefix}'");
                return StatusCode(500, "内部服务器错误");
            }
        }

        [HttpGet("topics/match")]
        public async Task<ActionResult<IEnumerable<string>>> MatchTopics([FromQuery] string prefix)
        {
            try
            {
                _logger.LogInformation($"匹配主题请求: 前缀 '{prefix}'");

                // 确保匹配器已初始化
                await EnsureMatchersInitializedAsync();

                var matched = _prefixMatcherService.MatchTopicPrefix(prefix);
                return Ok(matched);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"匹配主题失败: 前缀 '{prefix}'");
                return StatusCode(500, "内部服务器错误");
            }
        }
    }
}