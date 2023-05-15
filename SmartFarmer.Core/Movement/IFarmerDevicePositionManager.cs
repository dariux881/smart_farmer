using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Position;

namespace SmartFarmer.Movement;

public interface IFarmerDevicePositionManager : 
    IFarmerMoveOnGridDevice,
    IFarmerMoveAtHeightDevice,
    IFarmerTurnToolDevice
{
    Farmer5dPoint DevicePosition { get; }
    Task<bool> MoveToPosition(Farmer5dPoint position, CancellationToken token);
}