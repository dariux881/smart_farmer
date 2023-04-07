using SmartFarmer.Misc;

namespace SmartFarmer.Tasks.Movement;

public class Farmer3dPositionNotifier : Farmer2dPositionNotifier, IFarmer3dPointNotifier
{
    private double _z;

    public double Z
    { 
        get => _z;
        set {
            if (_z != value)
            {
                _z = value;
                SendNewPoint();
            }
        }
    }
}