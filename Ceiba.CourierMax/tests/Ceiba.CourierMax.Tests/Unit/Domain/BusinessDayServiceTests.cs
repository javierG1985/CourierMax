using Ceiba.CourierMax.Domain.Services;

namespace Ceiba.CourierMax.Tests.Unit.Domain;

public class BusinessDayServiceTests
{
    [Theory]
    [InlineData(2026, 1, 2)]   // Viernes normal
    [InlineData(2026, 2, 2)]   // Lunes normal
    public void IsBusinessDay_OnRegularWeekday_ShouldReturnTrue(int year, int month, int day)
    {
        Assert.True(BusinessDayService.IsBusinessDay(new DateOnly(year, month, day)));
    }

    [Theory]
    [InlineData(2026, 1, 3)]   // Sábado
    [InlineData(2026, 1, 4)]   // Domingo
    [InlineData(2026, 1, 1)]   // Año nuevo (festivo)
    [InlineData(2026, 5, 1)]   // Día del trabajo (festivo)
    [InlineData(2026, 7, 20)]  // Independencia (festivo)
    [InlineData(2026, 12, 8)]  // Inmaculada Concepción (festivo)
    public void IsBusinessDay_OnWeekendOrHoliday_ShouldReturnFalse(int year, int month, int day)
    {
        Assert.False(BusinessDayService.IsBusinessDay(new DateOnly(year, month, day)));
    }

    [Fact]
    public void AddBusinessDays_FromFriday_ShouldSkipWeekend()
    {
        // Viernes 2026-01-02 + 1 día hábil = Lunes 2026-01-05
        var friday = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var result = BusinessDayService.AddBusinessDays(friday, 1);

        Assert.Equal(new DateOnly(2026, 1, 5), DateOnly.FromDateTime(result));
    }

    [Fact]
    public void AddBusinessDays_AroundHoliday_ShouldSkipIt()
    {
        // Jueves 2026-04-30 + 1 día hábil debe saltar el 01-May (festivo) → Lunes 04-May
        var thursday = new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc);
        var result = BusinessDayService.AddBusinessDays(thursday, 1);

        Assert.Equal(new DateOnly(2026, 5, 4), DateOnly.FromDateTime(result));
    }

    [Fact]
    public void CountBusinessDays_BetweenMondayAndFriday_ShouldReturn4()
    {
        // Lunes 05-Jan a Viernes 09-Jan = 4 días hábiles (lunes, martes, miércoles, jueves)
        var from = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc);
        var to   = new DateTime(2026, 1, 9, 0, 0, 0, DateTimeKind.Utc);

        Assert.Equal(4, BusinessDayService.CountBusinessDays(from, to));
    }
}
