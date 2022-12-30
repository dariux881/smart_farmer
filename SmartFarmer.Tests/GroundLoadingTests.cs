using NUnit.Framework;
using SmartFarmer.Tests.Utils;
using System.Collections.Generic;
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
            _ground = new FarmerGround();
        }

        [SetUp]
        public void Setup()
        {
            var basePath = Path.Combine(".", "Configuration");
            var plants = InformationLoader.LoadPlantsFromCsvFile(Path.Combine(basePath, "Plants.csv"));
            var plantsInGround = InformationLoader.LoadPlantInstanceFromCsvFile(Path.Combine(basePath, "PlantsInstance.csv"), plants);

            _ground.AddPlants(plantsInGround.ToArray());
        }

        [Test]
        public void GroundExists()
        {
            Assert.IsNotNull(_ground);
            Assert.IsNotEmpty(_ground.Plants);
        }
    }
}