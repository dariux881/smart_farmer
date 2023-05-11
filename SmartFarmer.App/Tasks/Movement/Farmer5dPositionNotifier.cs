using SmartFarmer.Misc;

namespace SmartFarmer.Tasks.Movement;

public class Farmer5dPositionNotifier : Farmer3dPositionNotifier, IFarmer5dPointNotifier
{
    private double _alpha, _beta;
    private object _setValueLock = new object();

    public double Alpha
    { 
        get { lock (_setValueLock) { return _alpha; } }
        set {
            lock (_setValueLock)
            {
                if (_alpha != value)
                {
                    _alpha = value;
                }
            }

            SendNewPoint();
        }
    }

    public double Beta
    { 
        get { lock (_setValueLock) { return _beta; } }
        set {
            lock (_setValueLock)
            {
                if (_beta != value)
                {
                    _beta = value;
                }
            }

            SendNewPoint();
        }
    }

    public override string ToString()
    {
        return $"{X} - {Y} - {Z} - {Alpha} - {Beta}";
    }
}