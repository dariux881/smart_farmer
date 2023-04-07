using System;

namespace SmartFarmer.Misc;

public interface IFarmerPointNotifier
{
    event EventHandler NewPoint;
}
