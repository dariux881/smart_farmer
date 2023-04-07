using System;

namespace SmartFarmer.Misc;

public interface IFarmer2dPointNotifier : IFarmerPointNotifier
{
    double X { get; }
    double Y { get; }
}