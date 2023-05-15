using System;

namespace SmartFarmer.Position;

public interface IFarmerPointNotifier
{
    event EventHandler NewPoint;
}
