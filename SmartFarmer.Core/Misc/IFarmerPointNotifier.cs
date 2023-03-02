using System;

public interface IFarmerPointNotifier
{
    double X { get; }
    double Y { get; }
    event EventHandler NewPoint;
}