namespace NewsInsight.Shared.Models.DTOs
{
    // 通用分页响应
    public class PaginatedResponse<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<T> Items { get; set; } = new List<T>();

        // 特定于浏览记录的额外字段
        public int? TotalDuration { get; set; } = null;

        public PaginatedResponse() { }

        public PaginatedResponse(List<T> items, int page, int pageSize, int totalCount, int? totalDuration = null)
        {
            Items = items;
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalDuration = totalDuration;
        }
    }

    // 新闻 DTO
    public class NewsDto
    {
        public int Id { get; set; }
        public string Headline { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public DateTime ReleasedTime { get; set; }
        public int BrowseCount { get; set; }
        public string Content { get; set; }
    }

    // 用户浏览记录 DTO
    public class BrowseRecordDto
    {
        public int UserId { get; set; }
        public int NewsId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public string NewsHeadline { get; set; } = string.Empty;
        public string NewsCategory { get; set; } = string.Empty;
    }

    // 每日统计 DTO
    public class DailyStatDto
    {
        public DateTime Date { get; set; }  // 每天的日期
        public int BrowseCount { get; set; }  // 每天的浏览次数
        public int TotalDuration { get; set; }  // 每天的总浏览时长
    }

    // 类别统计 DTO
    public class CategoryStatDto
    {
        public string Category { get; set; } = string.Empty;
        public int TotalBrowseCount { get; set; }
        public int TotalBrowseDuration { get; set; }
        public double AverageDuration { get; set; }
    }

    // 类别统计响应
    public class CategoryStatsResponse
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<CategoryStatDto> Categories { get; set; } = new List<CategoryStatDto>();
    }


    // 趋势点 DTO
    public class TrendPointDto
    {
        public DateTime Date { get; set; }
        public int BrowseCount { get; set; }
        public int BrowseDuration { get; set; }
    }

    // 类别趋势 DTO
    public class CategoryTrendDto
    {
        public string Category { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<TrendPointDto> TrendPoints { get; set; } = new List<TrendPointDto>();
    }

    // 用户兴趣分布 DTO
    public class UserInterestDistributionDto
    {
        public int UserId { get; set; }
        public List<InterestDistributionItemDto> Distribution { get; set; } = new List<InterestDistributionItemDto>();
    }

    // 用户兴趣趋势 DTO
    public class UserInterestTrendDto
    {
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<InterestTrendItemDto> TrendData { get; set; } = new List<InterestTrendItemDto>();
    }

    // 兴趣分布项 DTO
    public class InterestDistributionItemDto
    {
        public string Category { get; set; } = string.Empty;
        public int TotalClicks { get; set; }
        public int TotalDwellTime { get; set; }
    }

    // 兴趣趋势项 DTO
    public class InterestTrendItemDto
    {
        public string Category { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
        public int ClickCount { get; set; }
        public int DwellTime { get; set; }
    }

    // 每日趋势DTO
    public class DailyTrendDto
    {
        public DateTime Date { get; set; }
        public int BrowseCount { get; set; }
        public int TotalDuration { get; set; }
    }

    // 热点时段DTO
    public class HotPeriodDto
    {
        public int Hour { get; set; }
        public int BrowseCount { get; set; }
        public int TotalDuration { get; set; }
    }

    public class CategoryHeatmapDto
    {
        public DateTime StartDate { get; set; } // 查询的开始日期
        public DateTime EndDate { get; set; } // 查询的结束日期
        public List<CategoryHeatmapDayDto> HeatmapData { get; set; } // 按日期分组的类别热度数据
    }

    public class CategoryHeatmapDayDto
    {
        public DateTime Date { get; set; } // 每个日期
        public List<CategoryHeatItemDto> Categories { get; set; } // 每个日期下的类别数据
    }

    public class CategoryHeatItemDto
    {
        
        public string Category { get; set; } // 类别名称
        public int BrowseCount { get; set; } // 浏览次数
        public int BrowseDuration { get; set; } // 浏览时长
    }
}
