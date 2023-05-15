using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Misc;

public static class Extensions
{
    private static Random random = new Random();

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static string Encode(this string obj)
    {
        return Convert.ToBase64String(Encoding.ASCII.GetBytes(obj));
    }

    public static byte[] Decode(this string obj)
    {
        return Convert.FromBase64String(obj);
    }

    public static string Serialize(this object obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    public static T Deserialize<T>(this string obj)
    {
        return JsonConvert.DeserializeObject<T>(obj);
    }
    public static string RemoveAdditionalQuotes(this string text)
    {
        if (text != null && 
            text.StartsWith("\"") &&
            text.EndsWith("\""))
        {
            text = text.Substring(1, text.Length-2);
        }

        return text;
    }

    public static bool IsNumber(this object value)
    {
        return value is short
                || value is ushort
                || value is int
                || value is uint
                || value is long
                || value is ulong
                || value is float
                || value is double
                || value is decimal;
    }

    public static bool IsNan(this double value)
    {
        return double.IsNaN(value);
    }

    public static int GetInt(this object number)
    {
        int x;
        switch (number)
        {
            case string yStr:
                x = int.Parse(yStr, CultureInfo.InvariantCulture);
                break;
            default:
                if (number.IsNumber())
                {
                    x = (int)number;
                }
                else
                {
                    throw new InvalidCastException(number + " is not a valid number");
                }

                break;
        }

        return x;
    }

    public static double GetDouble(this object number)
    {
        double x;
        switch (number)
        {
            case string yStr:
                x = double.Parse(yStr, CultureInfo.InvariantCulture);
                break;
            default:
                if (number.IsNumber())
                {
                    x = (double)number;
                }
                else
                {
                    throw new InvalidCastException(number + " is not a valid number");
                }

                break;
        }

        return x;
    }

    public static string GetTaskName(this IFarmerTask task)
    {
        if (task == null) return null;
        return task.TaskName ?? task.GetType().FullName;
    }
}
