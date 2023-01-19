using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Generic
{
    public interface IFarmerPlanStep : IHasProgressCheckInfo, IFarmerService 
    {
        string TaskClassFullName { get; }
        TimeSpan Delay { get; }
        object[] BuildParameters { get; }

        Task Execute(object[] parameters, CancellationToken token);
    }
}
