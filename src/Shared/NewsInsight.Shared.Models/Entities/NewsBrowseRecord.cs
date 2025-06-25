using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NewsInsight.Api.Models
{
    [Table("t_news_browse_record")]
    public class NewsBrowseRecord
    {
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("news_id")]
        public int NewsId { get; set; }

        [Column("start_ts")]
        public int StartTs { get; set; }

        [Column("duration")]
        public int Duration { get; set; }

        [Column("start_day")]
        public int StartDay { get; set; }

        public News News { get; set; }
    }
}
