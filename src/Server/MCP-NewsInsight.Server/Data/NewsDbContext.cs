using Microsoft.EntityFrameworkCore;

namespace MCPNewsInsight.Server.Data
{
    public class NewsDbContext : DbContext
    {
        public NewsDbContext(DbContextOptions<NewsDbContext> options) : base(options) { }

        public DbSet<News> News { get; set; }
        public DbSet<NewsBrowseRecord> NewsBrowseRecord { get; set; }
    }

    public class News
    {
        public int Id { get; set; }

        public string? Headline { get; set; }
        public string? Content { get; set; }
        public string? Category { get; set; }
        public string? Topic { get; set; }
    }

    public class NewsBrowseRecord
    {
        public int UserId { get; set; }
        public int NewsId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public DateTime StartDay { get; set; }
    }
}
