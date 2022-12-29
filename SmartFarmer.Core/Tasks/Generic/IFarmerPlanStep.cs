using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Generic
{
    public interface IFarmerPlanStep : IHasProgressCheckInfo
    {
        IFarmerTask Job { get; }
        TimeSpan Delay { get; }

        Task Execute(object[] parameters, CancellationToken token);
    }
}
