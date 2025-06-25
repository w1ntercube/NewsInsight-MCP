using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NewsInsight.Api.Models
{
    [Table("t_news")] 
    public class News
    {
        [Column("news_id")] 
        public int NewsId { get; set; }

        [Column("headline")] 
        [Required]
        public string Headline { get; set; } = string.Empty;

        [Column("content")]  
        [Required]
        public string Content { get; set; } = string.Empty;

        [Column("category")] 
        [Required]
        public string Category { get; set; } = string.Empty;

        [Column("topic")]  
        public string Topic { get; set; } = string.Empty;

        [Column("total_browse_num")] 
        public int TotalBrowseNum { get; set; }

        [Column("total_browse_duration")]
        public int TotalBrowseDuration { get; set; }

        [Column("released_time")] 
        public int ReleasedTime { get; set; }
    }
}
