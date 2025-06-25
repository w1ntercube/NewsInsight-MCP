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
    public class UserInterestController : ControllerBase
    {
        private readonly NewsDbContext _context;

        public UserInterestController(NewsDbContext context)
        {
            _context = context;
        }

        // 获取用户兴趣分布
        [HttpGet("user-interest-distribution/{userId}")]
        public async Task<ActionResult<UserInterestDistributionDto>> GetUserInterestDistribution(
            int userId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var query = _context.UserInterests
                .Where(ui => ui.UserId == userId);

            if (startDate.HasValue)
            {
                var startTs = DateTimeUtils.ConvertToUnixTimestamp(startDate.Value);
                query = query.Where(ui => ui.UpdateTime >= startTs);
            }

            if (endDate.HasValue)
            {
                var endTs = DateTimeUtils.ConvertToUnixTimestamp(endDate.Value);
                query = query.Where(ui => ui.UpdateTime <= endTs);
            }

            var distribution = await query
                .GroupBy(ui => ui.Category)
                .Select(g => new InterestDistributionItemDto
                {
                    Category = g.Key,
                    TotalClicks = g.Sum(ui => ui.ClickCount),
                    TotalDwellTime = g.Sum(ui => ui.DwellTime)
                })
                .ToListAsync();

            return Ok(new UserInterestDistributionDto
            {
                UserId = userId,
                Distribution = distribution
            });
        }

    }
}
