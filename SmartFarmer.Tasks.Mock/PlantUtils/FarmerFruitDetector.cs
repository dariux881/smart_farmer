using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Base;

namespace SmartFarmer.Tasks.PlantUtils
{
    public class FarmerFruitDetector : FarmerBaseTask, IFarmerFruitDetector
    {
        public override Task Execute(object[] parameters, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}