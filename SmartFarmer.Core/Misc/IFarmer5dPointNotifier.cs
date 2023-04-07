namespace SmartFarmer.Misc;

public interface IFarmer5dPointNotifier : IFarmer3dPointNotifier
{
    double Alpha { get; }
    double Beta { get; }
}