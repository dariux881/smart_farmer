using System;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks
{
    public interface IFarmerPlanStep : IHasProgressCheckInfo
    {
        IFarmerTask Job { get; }
        TimeSpan Delay { get; }
    }
}
