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
    public class NewsCategoryController : ControllerBase
    {
        private readonly NewsDbContext _context;

        public NewsCategoryController(NewsDbContext context)
        {
            _context = context;
        }

        // 获取类别热度分析
        [HttpGet("category-heatmap")]
        public async Task<ActionResult<CategoryHeatmapDto>> GetCategoryHeatmap(
            [FromQuery] List<string> categories, // 支持多个类别
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            // 将日期转换为天戳
            var startStamp = DateTimeUtils.ConvertToDayStamp(startDate);
            var endStamp = DateTimeUtils.ConvertToDayStamp(endDate);

            // 打印转换后的天戳（用于调试）
            Console.WriteLine($"Start Stamp: {startStamp}, End Stamp: {endStamp}");

            // 查询多个类别在日期范围内的数据
            var heatmapData = await _context.NewsCategories
                .Where(nc => categories.Contains(nc.Category) && nc.DayStamp >= startStamp && nc.DayStamp <= endStamp)
                .GroupBy(nc => new { nc.DayStamp, nc.Category })  // 按日期和类别分组
                .Select(g => new
                {
                    DayStamp = g.Key.DayStamp,
                    Category = g.Key.Category,
                    BrowseCount = g.Sum(nc => nc.BrowseCount),
                    BrowseDuration = g.Sum(nc => nc.BrowseDuration)
                })
                .OrderBy(g => g.DayStamp)
                .ToListAsync();

            // 调试
            Console.WriteLine($"Fetched Data: {heatmapData.Count}");

            // 格式化查询结果为按日期分组的类别热度数据
            var heatmap = heatmapData
                .GroupBy(d => d.DayStamp)
                .Select(dayGroup => new CategoryHeatmapDayDto
                {
                    Date = DateTimeUtils.ConvertFromDayStamp(dayGroup.Key),
                    Categories = dayGroup.Select(c => new CategoryHeatItemDto
                    {
                        Category = c.Category,
                        BrowseCount = c.BrowseCount,
                        BrowseDuration = c.BrowseDuration
                    }).ToList()
                })
                .ToList();

            return Ok(new CategoryHeatmapDto
            {
                StartDate = startDate,
                EndDate = endDate,
                HeatmapData = heatmap
            });
        }


    }
}
