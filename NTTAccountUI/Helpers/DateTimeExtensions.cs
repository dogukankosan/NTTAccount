// Helpers/DateTimeExtensions.cs
namespace NTTAccountUI.Helpers;

public static class DateTimeExtensions
{
    private static readonly TimeZoneInfo TurkeyZone =
        TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
    public static DateTime ToTurkeyTime(this DateTime utcTime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(utcTime, DateTimeKind.Utc), TurkeyZone);
    }
    public static DateTime? ToTurkeyTime(this DateTime? utcTime)
    {
        if (!utcTime.HasValue) return null;
        return utcTime.Value.ToTurkeyTime();
    }
    public static string ToTurkeyTimeString(this DateTime utcTime, string format = "dd.MM.yyyy HH:mm")
    {
        return utcTime.ToTurkeyTime().ToString(format);
    }
    public static string ToTurkeyTimeString(this DateTime? utcTime, string format = "dd.MM.yyyy HH:mm")
    {
        if (!utcTime.HasValue) return "-";
        return utcTime.Value.ToTurkeyTimeString(format);
    }
}