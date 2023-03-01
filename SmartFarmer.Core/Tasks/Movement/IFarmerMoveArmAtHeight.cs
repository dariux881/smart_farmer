using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tasks.Movement
{
    public interface IFarmerMoveArmAtHeight : IFarmerTask
    {
        Task MoveToHeight(int heightInCm, CancellationToken token);
        int GetCurrentHeight();
    }
}