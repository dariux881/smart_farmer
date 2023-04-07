using System;
using SmartFarmer.Misc;

namespace SmartFarmer.Tasks.Movement;

public class Farmer2dPositionNotifier : IFarmer2dPointNotifier
{
    private double _x, _y;

    public double X 
    { 
        get => _x;
        set {
            if (_x != value)
            {
                _x = value;
                SendNewPoint();
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
                SendNewPoint();
            }
        }
    }

    public event EventHandler NewPoint;

    protected void SendNewPoint()
    {
        NewPoint?.Invoke(this, EventArgs.Empty);
    }
}