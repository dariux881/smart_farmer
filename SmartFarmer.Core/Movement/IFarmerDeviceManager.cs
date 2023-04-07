using SmartFarmer.Misc;

namespace SmartFarmer.Movement;

public interface IFarmerDeviceManager : 
    IFarmerMoveOnGridDevice,
    IFarmerMoveAtHeightDevice,
    IFarmerTurnToolDevice,
    IFarmer5dPointNotifier
{

}