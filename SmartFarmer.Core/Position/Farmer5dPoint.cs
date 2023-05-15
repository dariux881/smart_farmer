
namespace SmartFarmer.Position;

public class Farmer5dPoint : Farmer3dPoint
{
    private double _alpha, _beta;
    private object _setValueLock = new object();

    public Farmer5dPoint()
        : this(double.NaN, double.NaN, double.NaN, double.NaN, double.NaN)
    {

    }
    
    public Farmer5dPoint(double x, double y, double z, double alpha, double beta)
        : this(x, y, z, alpha, beta, null, null)
    {
    }

    public Farmer5dPoint(double x, double y, double z, double alpha, double beta, double? maxWidth, double? maxLength)
        : base(x, y, z, maxWidth, maxLength)
    {
        Alpha = alpha;
        Beta = beta;
    }

    public Farmer5dPoint(Farmer5dPoint position)
    {
        X = position.X;
        Y = position.Y;
        Z = position.Z;
        Alpha = position.Alpha;
        Beta = position.Beta;
    }

    public double Alpha 
    { 
        get { lock (_setValueLock) { return _alpha; } }
        set {
            var notify = false;
            lock (_setValueLock)
            {
                if (_alpha != value)
                {
                    _alpha = value;
                    notify = true;
                }
            }

            if (notify) SendNewPoint();
        }
    }

    public double Beta 
    { 
        get { lock (_setValueLock) { return _beta; } }
        set {
            var notify = false;
            lock (_setValueLock)
            {
                if (_beta != value)
                {
                    _beta = value;
                    notify = true;
                }
            }

            if (notify) SendNewPoint();
        }
    }
}
