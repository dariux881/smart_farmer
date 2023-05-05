using System;
using SmartFarmer.Misc;

namespace SmartFarmer.Tasks.Movement;

public class Farmer2dPositionNotifier : IFarmer2dPointNotifier
{
    private double _x, _y;
    private object _setValueLock = new object();

    public double X 
    { 
        get { lock (_setValueLock) { return _x; } }
        set {
            lock (_setValueLock)
            {
                if (_x != value)
                {
                    _x = value;
                }
            }

            SendNewPoint();
        }
    }

    public double Y 
    { 
        get { lock (_setValueLock) { return _y; } }
        set {
            lock (_setValueLock)
            {
                if (_y != value)
                {
                    _y = value;
                }
            }

            SendNewPoint();
        }
    }

    public event EventHandler NewPoint;

    protected void SendNewPoint()
    {
        NewPoint?.Invoke(this, EventArgs.Empty);
    }
}