using SmartFarmer.Misc;

public class Farmer5dPoint : IFarmer5dPoint
{
    public Farmer5dPoint(double x, double y, double z, double alpha, double beta)
    {
        X = x;
        Y = y;
        Z = z;
        Alpha = alpha;
        Beta = beta;
    }

    public double X { get; private set; }
    public double Y { get; private set; }
    public double Z { get; private set; }
    public double Alpha { get; private set; }
    public double Beta { get; private set; }
}
