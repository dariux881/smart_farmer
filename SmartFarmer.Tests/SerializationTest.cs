using Newtonsoft.Json;
using NUnit.Framework;
using SmartFarmer.Alerts;
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
            _ground = new FarmerGround(
                "test_ground_id",
                "ground_name",
                10.0,
                11.0,
                10,
                20,
                "user1",
                null,
                FarmerPlantInstanceProvider.Instance, 
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
                FarmerPlantProvider.Instance.AddFarmerPlant(plant);
            }

            var plantsInGround = InformationLoader.LoadPlantInstanceFromCsvFile(Path.Combine(basePath, "PlantsInstance.csv"));

            foreach (var plant in plantsInGround)
            {
                FarmerPlantInstanceProvider.Instance.AddFarmerPlantInstance(plant);
            }

            _ground.AddPlants(plantsInGround.Select(x => x.ID).ToArray());
        }

        [Test]
        public void PlantSerialization_ExpectedNotEmpty()
        {
            var plant = FarmerPlantInstanceProvider.Instance.GetFarmerPlantInstance(_ground.PlantIds.First());

            var jsonPlant = JsonConvert.SerializeObject(plant, Formatting.Indented);
            Assert.IsNotNull(jsonPlant);
        }

        [Test]
        public void PlantDeserialization_ExpectedSame()
        {
            var plant = FarmerPlantInstanceProvider.Instance.GetFarmerPlantInstance(_ground.PlantIds.Last());

            var jsonPlant = JsonConvert.SerializeObject(plant, Formatting.Indented);
            Assert.IsNotNull(jsonPlant);

            var deserializedPlant = JsonConvert.DeserializeObject<FarmerPlantInstance>(jsonPlant);

            Assert.IsNotNull(deserializedPlant);
            Assert.AreEqual(plant.ID, deserializedPlant.ID);
            Assert.AreEqual(plant.PlantName, deserializedPlant.PlantName);
            Assert.AreEqual(plant.Plant.ID, deserializedPlant.PlantKindID);
        }

        [Test]
        public void GroundSerialization_ExpectedNotNull()
        {
            var jsonGround = JsonConvert.SerializeObject(_ground, Formatting.Indented);
            Assert.IsNotNull(jsonGround);
        }

        //TODO de-serializa plans

        [Test]
        public void GroundDeserialization_ExpectedSame()
        {
            var jsonGround = JsonConvert.SerializeObject(_ground, Formatting.Indented);
            Assert.IsNotNull(jsonGround);

            var deserializedGround = JsonConvert.DeserializeObject<FarmerGround>(jsonGround);

            Assert.IsNotNull(deserializedGround);
            Assert.AreEqual(_ground.ID, deserializedGround.ID);
            Assert.AreEqual(_ground.GroundName, deserializedGround.GroundName);
            Assert.AreEqual(_ground.Latitude, deserializedGround.Latitude);
            Assert.AreEqual(_ground.Longitude, deserializedGround.Longitude);
            Assert.AreEqual(_ground.WidthInMeters, deserializedGround.WidthInMeters);
            Assert.AreEqual(_ground.LengthInMeters, deserializedGround.LengthInMeters);
            Assert.AreEqual(_ground.UserID, deserializedGround.UserID);
            Assert.AreEqual(_ground.PlantIds.Count, deserializedGround.Plants.Count);

            for (int i = 0; i < _ground.PlantIds.Count; i++)
            {
                IFarmerPlantInstance plant = FarmerPlantInstanceProvider.Instance.GetFarmerPlantInstance(_ground.PlantIds[i]);
                Assert.AreEqual(plant.ID, _ground.PlantIds[i]);
            }

            Assert.AreEqual(_ground.Plans.Count, deserializedGround.Plans.Count);        
        }
    }
}