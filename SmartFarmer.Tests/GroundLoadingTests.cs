using NUnit.Framework;
using SmartFarmer.Alerts;
using SmartFarmer.Tasks;
using SmartFarmer.Tests.Utils;
using SmartFarmer.Utils;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SmartFarmer.Tests
{
    [TestFixture]
    public class GroundLoadingTests
    {
        private IFarmerGround _ground;

        public GroundLoadingTests() 
        {
            _ground = new FarmerGround(
                FarmerPlantProvider.Instance, 
                FarmerIrrigationInfoProvider.Instance, 
                FarmerPlantInstanceProvider.Instance, 
                FarmerPlanProvider.Instance, 
                FarmerAlertProvider.Instance, 
                FarmerAlertHandler.Instance,
                false);
        }

        [SetUp]
        public async Task Setup()
        {
            var basePath = Path.Combine(".", "Configuration");
            var plants = await InformationLoader.LoadPlantsFromCsvFile(Path.Combine(basePath, "Plants.csv"));

            foreach (var plant in plants)
            {
                await FarmerPlantProvider.Instance.AddFarmerService(plant);
            }

            var plantsInGround = await InformationLoader.LoadPlantInstanceFromCsvFile(Path.Combine(basePath, "PlantsInstance.csv"));
            
            foreach (var plant in plantsInGround)
            {
                await FarmerPlantInstanceProvider.Instance.AddFarmerService(plant);
            }
            
            _ground.AddPlants(plantsInGround.Select(x => x.ID).ToArray());
        }

        [Test]
        public void GroundExists()
        {
            Assert.IsNotNull(_ground);
            Assert.IsNotEmpty(_ground.PlantIds);
        }
    }
}