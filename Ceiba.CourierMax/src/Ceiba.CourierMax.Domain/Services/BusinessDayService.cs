namespace Ceiba.CourierMax.Domain.Services;

public static class BusinessDayService
{
    // Festivos colombianos 2026 (RN-02)
    private static readonly HashSet<DateOnly> Holidays2026 =
    [
        new(2026, 1, 1),
        new(2026, 1, 26),
        new(2026, 3, 30),
        new(2026, 4, 2),
        new(2026, 4, 3),
        new(2026, 5, 1),
        new(2026, 6, 1),
        new(2026, 6, 29),
        new(2026, 7, 20),
        new(2026, 8, 17),
        new(2026, 10, 19),
        new(2026, 11, 2),
        new(2026, 11, 16),
        new(2026, 12, 8)
    ];

    public static bool IsBusinessDay(DateOnly date)
    {
        return date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday)
               && !Holidays2026.Contains(date);
    }

    public static int CountBusinessDays(DateTime from, DateTime to)
    {
        var start = DateOnly.FromDateTime(from);
        var end = DateOnly.FromDateTime(to);
        var count = 0;

        var current = start;
        while (current < end)
        {
            if (IsBusinessDay(current))
                count++;
            current = current.AddDays(1);
        }

        return count;
    }

    public static DateTime AddBusinessDays(DateTime from, int days)
    {
        var current = DateOnly.FromDateTime(from);
        var added = 0;

        while (added < days)
        {
            current = current.AddDays(1);
            if (IsBusinessDay(current))
                added++;
        }

        return current.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
    }
}
