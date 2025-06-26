using Microsoft.EntityFrameworkCore;
using NewsInsight.Api.Models;

namespace NewsInsight.Api.Data
{
    public class NewsDbContext : DbContext
    {
        public NewsDbContext(DbContextOptions<NewsDbContext> options) : base(options) { }

        // 创建新的独立实例
        public NewsDbContext CreateNewInstance()
        {
            var optionsBuilder = new DbContextOptionsBuilder<NewsDbContext>();
            optionsBuilder.UseMySql(
                Database.GetConnectionString(),
                ServerVersion.AutoDetect(Database.GetConnectionString()));

            return new NewsDbContext(optionsBuilder.Options);
        }

        public DbSet<News> News { get; set; }
        public DbSet<NewsBrowseRecord> NewsBrowseRecords { get; set; }
        public DbSet<NewsCategory> NewsCategories { get; set; }
        public DbSet<UserInterest> UserInterests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 配置复合主键 - NewsBrowseRecord (user_id, news_id)
            modelBuilder.Entity<NewsBrowseRecord>()
                .HasKey(nbr => new { nbr.UserId, nbr.NewsId });

            // 配置复合主键 - NewsCategory (day_stamp, category)
            modelBuilder.Entity<NewsCategory>()
                .HasKey(nc => new { nc.DayStamp, nc.Category });

            // 配置复合主键 - UserInterest (user_id, category)
            modelBuilder.Entity<UserInterest>()
                .HasKey(ui => new { ui.UserId, ui.Category });

            base.OnModelCreating(modelBuilder);
        }
    }
}
