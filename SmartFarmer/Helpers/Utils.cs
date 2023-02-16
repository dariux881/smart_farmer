using System;

namespace SmartFarmer.Helpers;

public class Utils
{
    public static int GetCellsFromMeters(double meters)
    {
        return (int)Math.Floor(meters / Constants.METERS_IN_CELL);
    }
}