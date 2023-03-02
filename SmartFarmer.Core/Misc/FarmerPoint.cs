using System;

public class FarmerPoint : IDisposable
{
    private IFarmerPointNotifier _updater;

    public FarmerPoint(double x, double y, IFarmerPointNotifier updater, double? maxWidth = null, double? maxLength = null)
    {
        this.X = x;
        this.Y = y;

        this.MaxWidth = maxWidth;
        this.MaxLength = maxLength;

        _updater = updater;
        if (_updater != null)
        {
            _updater.NewPoint += UpdatePoint;
        }
    }

    public double X { get; private set; }

    public double Y { get; private set; }

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
        if (_updater != null)
        {
            _updater.NewPoint -= UpdatePoint;
        }
    }
    
    private void UpdatePoint(object sender, EventArgs e)
    {
        if (sender is not IFarmerPointNotifier notifier)
        {
            return;
        }

        this.X = notifier.X;
        this.Y = notifier.Y;
    }

}