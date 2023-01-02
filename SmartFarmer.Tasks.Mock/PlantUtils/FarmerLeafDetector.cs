using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Base;

namespace SmartFarmer.Tasks.PlantUtils
{
    public class FarmerLeafDetector : FarmerBaseTask, IFarmerLeafDetector
    {
        public override Task Execute(object[] parameters, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}