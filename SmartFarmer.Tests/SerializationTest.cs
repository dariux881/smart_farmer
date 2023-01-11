using Newtonsoft.Json;
using NUnit.Framework;
using SmartFarmer.Plants;
using SmartFarmer.Tests.Utils;
using SmartFarmer.Utils;
using System.IO;
using System.Linq;

namespace SmartFarmer.Tests
{
    [TestFixture]
    public class SerializationTest
    {
        private IFarmerGround _ground;

        public SerializationTest() 
        {
            _ground = new FarmerGround(false);
        }

        [SetUp]
        public void Setup()
        {
            var basePath = Path.Combine(".", "Configuration");
            var plants = InformationLoader.LoadPlantsFromCsvFile(Path.Combine(basePath, "Plants.csv"));
            
            foreach (var plant in plants)
            {
                FarmerPlantProvider.Instance.AddFarmerPlant(plant);
            }

            var plantsInGround = InformationLoader.LoadPlantInstanceFromCsvFile(Path.Combine(basePath, "PlantsInstance.csv"));

            _ground.AddPlants(plantsInGround.ToArray());
        }

        [Test]
        public void PlantSerialization_ExpectedNotEmpty()
        {
            var plant = _ground.Plants.First();

            var jsonPlant = JsonConvert.SerializeObject(plant, Formatting.Indented);
            Assert.IsNotNull(jsonPlant);
        }

        [Test]
        public void PlantDeserialization_ExpectedSame()
        {
            var plant = _ground.Plants.First();

            var jsonPlant = JsonConvert.SerializeObject(plant, Formatting.Indented);
            Assert.IsNotNull(jsonPlant);

            var deserializedPlant = JsonConvert.DeserializeObject<FarmerPlantInstance>(jsonPlant);

            Assert.IsNotNull(deserializedPlant);
            Assert.AreEqual(plant.ID, deserializedPlant.ID);
            Assert.AreEqual(plant.PlantName, deserializedPlant.PlantName);
            Assert.AreEqual(plant.Plant.ID, deserializedPlant.PlantKindID);
        }

        [Test]
        public void GroundSerialization_Test()
        {
            var jsonGround = JsonConvert.SerializeObject(_ground, Formatting.Indented);
            Assert.IsNotNull(jsonGround);
        }
    }
}