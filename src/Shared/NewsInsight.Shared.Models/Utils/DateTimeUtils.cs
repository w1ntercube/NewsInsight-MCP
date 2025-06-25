namespace NewsInsight.Shared.Models.Utils
{
    public static class DateTimeUtils
    {
        // 将 DateTime 转换为 Unix 时间戳（秒）
        public static int ConvertToUnixTimestamp(DateTime dateTime)
        {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (int)(dateTime.ToUniversalTime() - unixEpoch).TotalSeconds;
        }

        // 将 Unix 时间戳转换为 DateTime
        public static DateTime ConvertFromUnixTimestamp(int timestamp)
        {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return unixEpoch.AddSeconds(timestamp).ToLocalTime();
        }

        // 将 DateTime 转换为自 1970-01-01 起的天数
        public static int ConvertToDayStamp(DateTime date)
        {
            var unixEpoch = new DateTime(1970, 1, 1);  // 1970年1月1日作为基准
            return (int)(date.Date - unixEpoch).TotalDays;  // 计算自1970-01-01以来的天数
        }

        // 将天数转换回 DateTime
        public static DateTime ConvertFromDayStamp(int dayStamp)
        {
            var unixEpoch = new DateTime(1970, 1, 1);  // 1970年1月1日作为基准
            return unixEpoch.AddDays(dayStamp);  // 加上对应的天数，返回 DateTime
        }
    }
}
