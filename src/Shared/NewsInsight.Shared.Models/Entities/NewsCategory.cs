using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NewsInsight.Api.Models
{
    [Table("t_news_daily_category")] 
    public class NewsCategory
    {
        [Column("day_stamp")]  
        public int DayStamp { get; set; }

        [Column("category")]  
        [Required]
        public string Category { get; set; } = string.Empty;

        [Column("browse_count")] 
        public int BrowseCount { get; set; }

        [Column("browse_duration")]  
        public int BrowseDuration { get; set; }
    }
}
