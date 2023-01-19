using NUnit.Framework;
using SmartFarmer.Alerts;
using SmartFarmer.Tests.Utils;
using SmartFarmer.Utils;
using System.IO;
using System.Linq;

namespace SmartFarmer.Tests
{
    [TestFixture]
    public class GroundLoadingTests
    {
        private IFarmerGround _ground;

        public GroundLoadingTests() 
        {
            _ground = new FarmerGround(
                FarmerPlantInstanceProvider.Instance, 
                FarmerPlanProvider.Instance, 
                FarmerAlertHandler.Instance,
                false);
        }

        [SetUp]
        public void Setup()
        {
            var basePath = Path.Combine(".", "Configuration");
            var plants = InformationLoader.LoadPlantsFromCsvFile(Path.Combine(basePath, "Plants.csv"));

            foreach (var plant in plants)
            {
                FarmerPlantProvider.Instance.AddFarmerService(plant);
            }

            var plantsInGround = InformationLoader.LoadPlantInstanceFromCsvFile(Path.Combine(basePath, "PlantsInstance.csv"));

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