using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Base;

namespace SmartFarmer.Tasks.Weed
{
    public class FarmerWeedRemovalTask : FarmerBaseTask, IFarmerWeedRemovalTask
    {
        public override Task Execute(object[] parameters, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
