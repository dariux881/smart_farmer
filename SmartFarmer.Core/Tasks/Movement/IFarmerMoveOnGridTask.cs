using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tasks.Movement;

public interface IFarmerMoveOnGridTask : IFarmerTask
{
    Task MoveToPosition(int x, int y, CancellationToken token);
    void GetCurrentPosition(out int x, out int y);
}