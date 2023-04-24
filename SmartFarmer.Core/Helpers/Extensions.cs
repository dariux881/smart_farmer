using System;
using System.Globalization;

namespace SmartFarmer.Helpers;

public static class Extensions
{
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

}