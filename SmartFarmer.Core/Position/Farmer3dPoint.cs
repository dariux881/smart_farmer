
namespace SmartFarmer.Position;

public class Farmer3dPoint : Farmer2dPoint
{
    private double _z;
    private object _setValueLock = new object();
    
    public Farmer3dPoint()
        : this(double.NaN, double.NaN, double.NaN)
    {

    }

    public Farmer3dPoint(double x, double y, double z)
        : this(x, y, z, null, null)
    {
        this.X = x;
        this.Y = y;
    }

    public Farmer3dPoint(double x, double y, double z, double? maxWidth = null, double? maxLength = null)
        : base(x, y, maxWidth, maxLength)
    {
        this.Z = z;
    }

    
    public double Z
    { 
        get { lock (_setValueLock) { return _z; } }
        set {
            var notify = false;
            lock (_setValueLock)
            {
                if (_z != value)
                {
                    _z = value;
                    notify = true;
                }
            }

            if (notify) SendNewPoint();
        }
    }
}
