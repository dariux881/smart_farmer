namespace SmartFarmer.Misc;

public interface IFarmer3dPointNotifier : IFarmer2dPointNotifier
{
    double Z { get; }
}