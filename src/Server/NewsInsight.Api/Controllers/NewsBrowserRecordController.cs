using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsInsight.Api.Data;
using NewsInsight.Api.Models;
using NewsInsight.Shared.Models.DTOs;
using NewsInsight.Shared.Models.Utils;

namespace NewsInsight.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsBrowseRecordController : ControllerBase
    {
        private readonly NewsDbContext _context;

        public NewsBrowseRecordController(NewsDbContext context)
        {
            _context = context;
        }

        // 获取用户浏览记录（分页）
        [HttpGet("user-records/{userId}")]
        public async Task<ActionResult<PaginatedResponse<BrowseRecordDto>>> GetUserRecords(
            int userId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _context.NewsBrowseRecords
                .Include(r => r.News)
                .Where(r => r.UserId == userId);

            if (startDate.HasValue)
            {
                var startTs = DateTimeUtils.ConvertToUnixTimestamp(startDate.Value);
                query = query.Where(r => r.StartTs >= startTs);
            }

            if (endDate.HasValue)
            {
                var endTs = DateTimeUtils.ConvertToUnixTimestamp(endDate.Value);
                query = query.Where(r => r.StartTs <= endTs);
            }

            var totalCount = await query.CountAsync();
            var records = await query
                .OrderByDescending(r => r.StartTs)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new BrowseRecordDto
                {
                    UserId = r.UserId,
                    NewsId = r.NewsId,
                    StartTime = DateTimeUtils.ConvertFromUnixTimestamp(r.StartTs),
                    Duration = r.Duration,
                    NewsHeadline = r.News.Headline,
                    NewsCategory = r.News.Category
                })
                .ToListAsync();

            return Ok(new PaginatedResponse<BrowseRecordDto>(records, page, pageSize, totalCount));
        }

        // 获取用户浏览趋势（每日）
        [HttpGet("user-daily-trend/{userId}")]
        public async Task<ActionResult<IEnumerable<DailyTrendDto>>> GetUserDailyTrend(
            int userId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            // 如果没有传入日期，则返回所有记录
            var query = _context.NewsBrowseRecords.Where(r => r.UserId == userId);

            // 处理日期范围，如果传入了开始日期
            if (startDate.HasValue)
            {
                var startStamp = DateTimeUtils.ConvertToDayStamp(startDate.Value);
                query = query.Where(r => r.StartDay >= startStamp);
            }

            // 处理结束日期，如果传入了结束日期
            if (endDate.HasValue)
            {
                var endStamp = DateTimeUtils.ConvertToDayStamp(endDate.Value);
                query = query.Where(r => r.StartDay <= endStamp);
            }

            // 执行查询，进行分组和聚合
            var dailyStatsQuery = await query
                .GroupBy(r => r.StartDay)
                .Select(g => new
                {
                    DayStamp = g.Key,
                    BrowseCount = g.Count(),
                    TotalDuration = g.Sum(r => r.Duration)
                })
                .OrderBy(d => d.DayStamp)
                .ToListAsync();

            // 转换日期格式，并返回数据
            var dailyStats = dailyStatsQuery.Select(d => new DailyTrendDto
            {
                Date = DateTimeUtils.ConvertFromDayStamp(d.DayStamp),
                BrowseCount = d.BrowseCount,
                TotalDuration = d.TotalDuration
            }).ToList();

            return Ok(dailyStats);
        }

    }
}
