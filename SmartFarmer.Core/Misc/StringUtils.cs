using System;
using System.Linq;
using Newtonsoft.Json;

namespace SmartFarmer.Misc;

public static class StringUtils
{
    private static Random random = new Random();

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
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
}
