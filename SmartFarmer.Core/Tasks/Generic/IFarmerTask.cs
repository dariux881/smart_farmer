﻿using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Generic
{
    public interface IFarmerTask : IHasProgressCheckInfo, IFarmerService
    {
        FarmerTool RequiredTool { get; }
        Task Execute(object[] parameters, CancellationToken token);
    }
}
