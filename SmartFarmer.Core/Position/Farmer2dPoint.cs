using System;

namespace SmartFarmer.Position;

public class Farmer2dPoint : IFarmerPointNotifier, IDisposable
{
    private double _x, _y;
    private object _setValueLock = new object();

    public Farmer2dPoint()
        : this(double.NaN, double.NaN)
    {

    }

    public Farmer2dPoint(double x, double y)
        : this(x, y, null, null)
    {
    }

    public Farmer2dPoint(double x, double y, double? maxWidth = null, double? maxLength = null)
    {
        this.X = x;
        this.Y = y;

        this.MaxWidth = maxWidth;
        this.MaxLength = maxLength;
    }

    public event EventHandler NewPoint;

    public double X 
    { 
        get { lock (_setValueLock) { return _x; } }
        set {
            var notify = false;
            lock (_setValueLock)
            {
                if (_x != value)
                {
                    _x = value;
                    notify = true;
                }
            }

            if (notify) SendNewPoint();
        }
    }

    public double Y 
    { 
        get { lock (_setValueLock) { return _y; } }
        set {
            var notify = false;
            lock (_setValueLock)
            {
                if (_y != value)
                {
                    _y = value;
                    notify = true;
                }
            }

            if (notify) SendNewPoint();
        }
    }

    public double? MaxWidth { get; private set; }

    public double? MaxLength { get; private set; }

    public double? PercentX => 
        (MaxWidth != null && MaxWidth > 0) ?
            X*100 / MaxWidth.Value :
            null;

    public double? PercentY => 
        (MaxLength != null && MaxLength > 0) ?
            Y*100 / MaxLength.Value :
            null;

    public void Dispose()
    {
    }
    
    protected void SendNewPoint()
    {
        NewPoint?.Invoke(this, EventArgs.Empty);
    }
}