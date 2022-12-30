using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Base
{
    public abstract class FarmerBaseTask : IFarmerTask
    {
        public FarmerTool RequiredTool { get; protected set; }
        public bool IsInProgress { get; protected set; }

        public Exception LastException { get; protected set; }

        public abstract Task Execute(object[] parameters, CancellationToken token);
    }
}