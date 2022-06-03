﻿using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks
{
    public interface IFarmerTask
    {
        FarmerTool RequiredTool { get; }
        Task Execute(object[] parameters, CancellationToken token);
    }
}
