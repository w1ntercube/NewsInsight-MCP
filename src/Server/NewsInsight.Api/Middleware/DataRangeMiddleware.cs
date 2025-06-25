namespace NewsInsight.Api.Middleware;
public class DataRangeMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly DateTime MinDate = new DateTime(2019, 6, 13);
    private static readonly DateTime MaxDate = new DateTime(2019, 7, 3);

    public DataRangeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Query.TryGetValue("startDate", out var startDateValues) &&
            DateTime.TryParse(startDateValues, out var startDate) &&
            startDate < MinDate)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync($"开始日期不能早于 {MinDate:yyyy-MM-dd}");
            return;
        }

        if (context.Request.Query.TryGetValue("endDate", out var endDateValues) &&
            DateTime.TryParse(endDateValues, out var endDate) &&
            endDate > MaxDate)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync($"结束日期不能晚于 {MaxDate:yyyy-MM-dd}");
            return;
        }

        await _next(context);
    }
}