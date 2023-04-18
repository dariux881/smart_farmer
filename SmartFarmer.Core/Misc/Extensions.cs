using System;
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

    public static string GetTaskName(this IFarmerTask task)
    {
        if (task == null) return null;
        return task.TaskName ?? task.GetType().FullName;
    }
}
