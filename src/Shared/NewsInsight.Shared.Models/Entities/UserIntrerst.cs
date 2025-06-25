using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NewsInsight.Api.Models
{
    [Table("t_user_interest")]
    public class UserInterest
    {
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("category")]
        [Required]
        public string Category { get; set; } = string.Empty;

        [Column("topic")]
        [Required]
        public string Topic { get; set; } = string.Empty;

        [Column("click_count")]
        public int ClickCount { get; set; }

        [Column("dwell_time")]
        public int DwellTime { get; set; }

        [Column("update_time")]
        public int UpdateTime { get; set; }
    }
}

