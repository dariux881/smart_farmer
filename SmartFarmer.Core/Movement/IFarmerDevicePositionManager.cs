using SmartFarmer.Misc;

namespace SmartFarmer.Movement;

public interface IFarmerDevicePositionManager : 
    IFarmerMoveOnGridDevice,
    IFarmerMoveAtHeightDevice,
    IFarmerTurnToolDevice,
    IFarmer5dPointNotifier
{

}