using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

/// <summary>
/// </summary>
public static class DateExtension
{
    /// <summary>
    /// Аналог php timestamp. Количество секунд с 1 января 1970 года.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static int Timestamp(this DateTime date)
    {
        return (int)(date.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
    }

    /// <summary>
    /// Аналог php microtime. Количество секунд с дробной частью в микросекундах с 1 января 1970 года.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static double Microtime(this DateTime date)
    {
        return (date.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
    }

    /// <summary>
    /// Возвращает дату и время на основе php timestamp. См. <see cref="DateExtension.Timestamp(DateTime)"/>.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static DateTime FromUnixtime(this DateTime date, double timestamp)
    {
        var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        return dtDateTime.AddSeconds(timestamp);
    }

    /// <summary>
    /// Возвращает дату и время на основе php timestamp. См. <see cref="DateExtension.Timestamp(DateTime)"/>.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static DateTime FromTimestamp(this int timestamp)
    {
        var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        return dtDateTime.AddSeconds(timestamp);
    }

    /// <summary>
    /// Возвращает номер недели в году на основе даты. Используется текущий формат даты и времени.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static int GetWeekOfYear(this DateTime date)
    {
        DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
        Calendar cal = dfi.Calendar;

        return cal.GetWeekOfYear(date, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
    }

}