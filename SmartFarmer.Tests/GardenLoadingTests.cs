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
    public class GardenLoadingTests
    {
        private IFarmerGarden _garden;

        public GardenLoadingTests() 
        {
            _garden = new FarmerGarden(
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

            var plantsInGarden = await InformationLoader.LoadPlantInstanceFromCsvFile(Path.Combine(basePath, "PlantsInstance.csv"));
            
            foreach (var plant in plantsInGarden)
            {
                await FarmerPlantInstanceProvider.Instance.AddFarmerService(plant);
            }
            
            _garden.AddPlants(plantsInGarden.Select(x => x.ID).ToArray());
        }

        [Test]
        public void GardenExists()
        {
            Assert.IsNotNull(_garden);
            Assert.IsNotEmpty(_garden.PlantIds);
        }
    }
}