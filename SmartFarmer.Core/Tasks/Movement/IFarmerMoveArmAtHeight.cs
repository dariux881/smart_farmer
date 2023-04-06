using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tasks.Movement
{
    public interface IFarmerMoveArmAtHeight : IFarmerTask
    {
        Task MoveToHeight(double heightInCm, CancellationToken token);
        double GetCurrentHeight();
    }
}