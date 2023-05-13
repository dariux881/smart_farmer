using SmartFarmer.Misc;

public class Farmer5dPoint : IFarmer5dPoint
{
    public Farmer5dPoint()
        : this(double.NaN, double.NaN, double.NaN, double.NaN, double.NaN)
    {

    }
    
    public Farmer5dPoint(double x, double y, double z, double alpha, double beta)
    {
        X = x;
        Y = y;
        Z = z;
        Alpha = alpha;
        Beta = beta;
    }

    public Farmer5dPoint(IFarmer5dPoint position)
    {
        X = position.X;
        Y = position.Y;
        Z = position.Z;
        Alpha = position.Alpha;
        Beta = position.Beta;
    }

    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double Alpha { get; set; }
    public double Beta { get; set; }
}
