using System;

namespace MediaWiz.Forums.Extensions
{
    public static class DateTimeExtensions
    {
        public static string GetRelativeDate(this DateTime date)
        {
            // takes a date and displays a relative thing
            // i.e 10minutes ago, 1 day ago...
            var now = DateTime.Now;

            if (date == DateTime.MinValue)
                return "never";
            var months = date.GetTotalMonthsFrom(now);

            var span = now.Subtract(date);
            if (months > 0)
            {
                if (months == 1)
                {
                    return "last month";
                }
                return $"{months} months ago";
            }
            else if (span.Days > 0)
            {
                if (span.Days == 1)
                {
                    return "yesterday";
                }
                else
                {
                    if (span.Days % 7 > 0)
                    {
                        return $"{span.Days} days ago";
                    }
                    else
                    {
                        return $"{span.Days/7} weeks ago";
                    }
                    
                }
            }
            else if (span.Hours > 0)
            {
                return $"{span.Hours} hour{(span.Hours > 1 ? "s" : "")} ago";
            }
            else if (span.Minutes > 0)
            {
                return $"{span.Minutes} minute{(span.Minutes > 1 ? "s" : "")} ago";
            }
            else
            {
                return "just now";
            }
        }

        public static int GetTotalMonthsFrom(this DateTime dt1, DateTime dt2)
        {
            DateTime earlyDate = dt1 > dt2 ? dt2.Date : dt1.Date;
            DateTime lateDate = dt1 > dt2 ? dt1.Date : dt2.Date;

            // Start with 1 month's difference and keep incrementing
            // until we overshoot the late date
            int monthsDiff = 1;
            while (earlyDate.AddMonths(monthsDiff) <= lateDate)
            {
                monthsDiff++;
            }

            return monthsDiff - 1;
        }

        public static string ToDisplayDate(this DateTime dt)
        {
            return dt.ToLocalTime().ToString("dd MMM, yyyy");
        }
    }
}