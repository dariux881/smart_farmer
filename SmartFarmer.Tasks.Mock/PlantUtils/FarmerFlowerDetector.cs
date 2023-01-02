using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Base;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.PlantUtils
{
    public class FarmerFlowerDetector : FarmerBaseTask, IFarmerFlowerDetector
    {
        public override Task Execute(object[] parameters, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}