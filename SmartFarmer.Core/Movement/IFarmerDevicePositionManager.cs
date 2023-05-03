using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;

namespace SmartFarmer.Movement;

public interface IFarmerDevicePositionManager : 
    IFarmerMoveOnGridDevice,
    IFarmerMoveAtHeightDevice,
    IFarmerTurnToolDevice,
    IFarmer5dPointNotifier
{
    Task<bool> MoveToPosition(IFarmer5dPoint position, CancellationToken token);
}