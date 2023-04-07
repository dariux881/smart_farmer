using SmartFarmer.Misc;

namespace SmartFarmer.Tasks.Movement;

public class Farmer5dPositionNotifier : Farmer3dPositionNotifier, IFarmer5dPointNotifier
{
    private double _alpha, _beta;

    public double Alpha
    { 
        get => _alpha;
        set {
            if (_alpha != value)
            {
                _alpha = value;
                SendNewPoint();
            }
        }
    }

    public double Beta
    { 
        get => _beta;
        set {
            if (_beta != value)
            {
                _beta = value;
                SendNewPoint();
            }
        }
    }
}