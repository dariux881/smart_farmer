using SmartFarmer.Misc;

namespace SmartFarmer.Tasks.Movement;

public class Farmer3dPositionNotifier : Farmer2dPositionNotifier, IFarmer3dPointNotifier
{
    private double _z;
    private object _setValueLock = new object();

    public double Z
    { 
        get { lock (_setValueLock) { return _z; } }
        set {
            lock (_setValueLock)
            {
                if (_z != value)
                {
                    _z = value;
                }
            }

            SendNewPoint();
        }
    }
}