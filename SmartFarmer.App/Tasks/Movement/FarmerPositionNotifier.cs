using System;

namespace SmartFarmer.Tasks.Movement;

public class FarmerPositionNotifier : IFarmerPointNotifier
{
    private double _x, _y;

    public double X 
    { 
        get => _x;
        set {
            if (_x != value)
            {
                _x = value;
                NewPoint?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public double Y 
    { 
        get => _y;
        set {
            if (_y != value)
            {
                _y = value;
                NewPoint?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public event EventHandler NewPoint;
}