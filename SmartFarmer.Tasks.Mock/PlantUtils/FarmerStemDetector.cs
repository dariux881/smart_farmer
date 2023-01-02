using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Base;

namespace SmartFarmer.Tasks.PlantUtils
{
    public class FarmerStemDetector : FarmerBaseTask, IFarmerStemDetector
    {
        public override Task Execute(object[] parameters, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}