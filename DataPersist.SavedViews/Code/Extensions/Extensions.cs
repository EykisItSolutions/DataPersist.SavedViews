using System.Globalization;

namespace DataPersist.SavedViews;

// Small set of helpful extension methods

public static class Extensions
{

    // checks if string is numeric

    public static bool IsNumeric(this string s)
    {
        return int.TryParse(s, out _);
    }

    // used for currency, remove all non-numbers

    public static string OnlyNumbers(this string s)
    {
        return string.Join("", s.Where(char.IsDigit));
    }

    public static decimal CurrencyToDecimal(this string currency)
    {
        if (string.IsNullOrEmpty(currency)) return 0;
        return decimal.Parse(currency, NumberStyles.AllowCurrencySymbol | NumberStyles.Number);
    }

    public static string ToCurrency(this decimal? amount)
    {
        if (amount == null || !amount.HasValue) return "";
        return amount.Value.ToCurrency();
    }

    public static string ToCurrency(this decimal amount)
    {
        return string.Format("{0:C0}", amount);
    }

    public static string ToDate(this DateTime? dt)
    {
        if (dt == null) return "";
        return dt.Value.ToDate();
    }

    public static string ToDate(this DateTime dt)
    {
        return string.Format("{0:d}", dt);
    }

    public static string ToGrade(this decimal? average)
    {
        if (average == null) return "n/a";
        return ((decimal)average).ToGrade();
    }

    public static string ToGrade(this decimal average)
    {
        return string.Format("{0:0.0}", average);
    }

    public static string ToLog(this string? s)
    {
        if (s == null) return "NULL";
        return "'" + s + "'";
    }

    public static string ToLog(this DateTime? dt)
    {
        if (dt == null) return "NULL";
        return string.Format("{0:'yyyy-MM-dd HH:mm:ss.fff'}", dt);
    }

    public static string ToLog(this bool b)
    {
        return b == true ? "1" : "0";
    }
    public static string ToLog(this bool? b)
    {
        if (b == null) return "NULL";
        return b == true ? "1" : "0";
    }

    public static string ToLog(this decimal? d)
    {
        if (d == null) return "NULL";
        return d.ToString()!;
    }

    public static string ToLog(this decimal d)
    {
        return d.ToString();
    }

    public static string ToLog(this int? i)
    {
        if (i == null) return "NULL";
        return i.ToString()!;
    }

    public static string ToLog(this int i)
    {
        return i.ToString();
    }

    public static string Pluralize(this int count, string singular, string plural)
    {
        return count == 1 ? "1 " + singular : count.ToString() + " " + plural;
    }

    public static string Escape(this string s)
    {
        if (string.IsNullOrEmpty(s)) return "";

        return s.Replace("'", "''");
    }

    public static int? GetId(this object obj, int? defaultId = null)
    {
        if (obj == null) return defaultId;

        if (int.TryParse(obj.ToString(), out int value))
            return value as int?;

        return defaultId;
    }

    public static int GetInt(this object obj, int defaultInt = 0)
    {
        if (obj == null) return defaultInt;

        if (int.TryParse(obj.ToString(), out int value))
            return value;

        return defaultInt;
    }

    // ** Iterator Design Pattern

    // foreach iterates over an enumerable collection

    public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
    {
        foreach (T item in enumeration)
        {
            action(item);
        }
    }

    // Truncates a string and appends ellipsis if beyond a given length

    public static string Ellipsify(this string s, int maxLength)
    {
        if (string.IsNullOrEmpty(s)) return "";
        if (s.Length <= maxLength) return s;

        return s.Substring(0, maxLength) + "...";
    }

    // Used for safely appending sql AND clauses

    public static string AND(this string clause, string condition)
    {
        if (string.IsNullOrEmpty(condition))
            return clause ?? "";

        if (clause == null)
            clause = "";

        if (!string.IsNullOrWhiteSpace(clause))
            clause += " AND ";

        return clause + condition;
    }

    // used for safely appending sql OR clauses

    public static string OR(this string clause, string condition)
    {
        if (string.IsNullOrEmpty(condition))
            return clause ?? "";

        if (clause == null)
            clause = "";

        if (!string.IsNullOrWhiteSpace(clause))
            clause += " OR ";

        return clause + condition;
    }

    public static string ANDTenant(this string clause, int? tenantId)
    {
        if (clause == null)
            clause = "";

        if (!string.IsNullOrWhiteSpace(clause))
            clause += " AND ";

        return clause + " TenantId = " + tenantId;
    }
}
