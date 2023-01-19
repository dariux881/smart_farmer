using Newtonsoft.Json;
using NUnit.Framework;
using SmartFarmer.Alerts;
using SmartFarmer.MockedTasks;
using SmartFarmer.MockedTasks.GenericCollection;
using SmartFarmer.Plants;
using SmartFarmer.Tasks;
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
                null,
                "user1",
                null,
                null,
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

            foreach (var plant in plantsInGround)
            {
                FarmerPlantInstanceProvider.Instance.AddFarmerService(plant);
            }

            _ground.AddPlants(plantsInGround.Select(x => x.ID).ToArray());
        }

        [Test]
        public void PlantSerialization_ExpectedNotEmpty()
        {
            var plant = FarmerPlantInstanceProvider.Instance.GetFarmerService(_ground.PlantIds.First());

            var jsonPlant = JsonConvert.SerializeObject(plant, Formatting.Indented);
            Assert.IsNotNull(jsonPlant);
        }

        [Test]
        public void PlantDeserialization_ExpectedSame()
        {
            var plant = FarmerPlantInstanceProvider.Instance.GetFarmerService(_ground.PlantIds.Last());

            var jsonPlant = JsonConvert.SerializeObject(plant, Formatting.Indented);
            Assert.IsNotNull(jsonPlant);

            var deserializedPlant = JsonConvert.DeserializeObject<FarmerPlantInstance>(jsonPlant);

            Assert.IsNotNull(deserializedPlant);
            Assert.AreEqual(plant.ID, deserializedPlant.ID);
            Assert.AreEqual(plant.PlantName, deserializedPlant.PlantName);
            Assert.AreEqual(plant.Plant.ID, deserializedPlant.PlantKindID);
        }

        [Test]
        public void PlanSerialization_ExpectedNotNull()
        {
            var plan = new BaseFarmerPlan("id", "name");

            var jsonPlan = JsonConvert.SerializeObject(plan, Formatting.Indented);
            Assert.IsNotNull(jsonPlan);
        }

        [Test]
        public void PlanSerialization_ExpectedStepFilled()
        {
            var plan = new BaseFarmerPlan("id", "name");
            plan.EditableSteps.Add(new FarmerPlanStep(new MockedCumulativeTask()));

            var jsonPlan = JsonConvert.SerializeObject(plan, Formatting.Indented);
            Assert.IsNotNull(jsonPlan);
        }

        [Test]
        public void GroundSerialization_ExpectedNotNull()
        {
            var jsonGround = JsonConvert.SerializeObject(_ground, Formatting.Indented);
            Assert.IsNotNull(jsonGround);
        }

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
            Assert.AreEqual(_ground.PlanIds.Count, deserializedGround.Plans.Count);

            for (int i = 0; i < _ground.PlantIds.Count; i++)
            {
                IFarmerPlantInstance plant = FarmerPlantInstanceProvider.Instance.GetFarmerService(_ground.PlantIds[i]);
                Assert.AreEqual(plant.ID, _ground.PlantIds[i]);
            }

            for (int i = 0; i < _ground.PlanIds.Count; i++)
            {
                var plan = FarmerPlanProvider.Instance.GetFarmerService(_ground.PlanIds[i]);
                Assert.AreEqual(plan.ID, _ground.PlanIds[i]);
            }

            Assert.AreEqual(_ground.PlanIds.Count, deserializedGround.PlanIds.Count);
        }
    }
}