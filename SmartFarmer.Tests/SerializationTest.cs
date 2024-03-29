using Newtonsoft.Json;
using NUnit.Framework;
using SmartFarmer.Alerts;
using SmartFarmer.MockedTasks;
using SmartFarmer.MockedTasks.GenericCollection;
using SmartFarmer.Plants;
using SmartFarmer.Tasks;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tests.Utils;
using SmartFarmer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartFarmer.Tests
{
    [TestFixture]
    public class SerializationTest
    {
        private IFarmerGround _ground;

        private class TestableFarmerTask : IFarmerTask
        {
            public FarmerTool RequiredTool { get; set; }

            public bool IsInProgress { get; set; }

            public Exception LastException { get; set; }

            public string ID { get; set; }

            public async Task Execute(object[] parameters, CancellationToken token)
            {
                await Task.CompletedTask;
            }
        }

        private class TestableFarmerPlan : IFarmerPlan
        {
            private List<IFarmerPlanStep> _steps;

            [JsonConstructor]
            public TestableFarmerPlan(string id, string name, string[] stepIds)
            {
                ID = id;
                Name = name;
                _steps = new List<IFarmerPlanStep>();

                if (stepIds != null)
                {
                    foreach (var stepId in stepIds)
                    {
                        var step = FarmerPlanStepProvider.Instance.GetFarmerService(stepId);
                        if (step == null) throw new InvalidDataException(stepId + " is not a valid step id");

                        _steps.Add(step);
                    }
                }
            }

            public string Name { get; set; }

            public IReadOnlyList<string> StepIds => _steps.Select(x => x.ID).ToList().AsReadOnly();
            public IReadOnlyList<IFarmerPlanStep> Steps => _steps.AsReadOnly();

            public bool IsInProgress  { get; set; }

            public Exception LastException  { get; set; }

            public string ID  { get; set; }

            public Task Execute(CancellationToken token)
            {
                throw new NotImplementedException();
            }
        }

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
                null,
                FarmerPlantInstanceProvider.Instance, 
                FarmerPlanProvider.Instance, 
                FarmerAlertProvider.Instance, 
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

            var jsonPlant = JsonConvert.SerializeObject(plant);
            Assert.IsNotNull(jsonPlant);
        }

        [Test]
        public void PlantDeserialization_ExpectedSame()
        {
            var plant = FarmerPlantInstanceProvider.Instance.GetFarmerService(_ground.PlantIds.Last());

            var jsonPlant = JsonConvert.SerializeObject(plant);
            Assert.IsNotNull(jsonPlant);

            var deserializedPlant = JsonConvert.DeserializeObject<FarmerPlantInstance>(jsonPlant);

            Assert.IsNotNull(deserializedPlant);
            Assert.AreEqual(plant.ID, deserializedPlant.ID);
            Assert.AreEqual(plant.PlantName, deserializedPlant.PlantName);
            Assert.AreEqual(plant.Plant.ID, deserializedPlant.PlantKindID);
        }

        [Test]
        public void TaskSerialization_ExpectedFilled()
        {
            var obj = new TestableFarmerTask()
            {
                ID = "abc",
                RequiredTool = FarmerTool.Water,
                IsInProgress = true,
                LastException = new ArgumentException()
            };

            var jsonObj = JsonConvert.SerializeObject(obj);
            Assert.IsNotNull(jsonObj);

            var deserializedObj = JsonConvert.DeserializeObject<TestableFarmerTask>(jsonObj);
            Assert.IsNotNull(deserializedObj);
            Assert.AreEqual(obj.ID, deserializedObj.ID);
            Assert.AreEqual(obj.RequiredTool, deserializedObj.RequiredTool);
            Assert.IsFalse(deserializedObj.IsInProgress);
            Assert.IsNull(deserializedObj.LastException);
        }

        [Test]
        public void PlanStepSerialization_ExpectedStepFilled()
        {
            var obj = new FarmerPlanStep("id", new MockedCumulativeTask(), new object[] {1, 2, 3, "test param"})
            {
                Delay = new System.TimeSpan(0, 0, 5),
            };

            var jsonObj = JsonConvert.SerializeObject(obj);
            Assert.IsNotNull(jsonObj);

            var deserializedObj = JsonConvert.DeserializeObject<FarmerPlanStep>(jsonObj);
            Assert.IsNotNull(deserializedObj);
            Assert.AreEqual(obj.ID, deserializedObj.ID);
            Assert.AreEqual(obj.Delay, deserializedObj.Delay);
            Assert.AreEqual(obj.BuildParameters, deserializedObj.BuildParameters);
            Assert.AreEqual(obj.TaskClassFullName, deserializedObj.TaskClassFullName);
            Assert.IsNull(deserializedObj.LastException);
            Assert.IsFalse(deserializedObj.IsInProgress);
        }

        [Test]
        public void PlanSerialization_ExpectedNotNull()
        {
            var obj = new BaseFarmerPlan("id", "name");

            var jsonObj = JsonConvert.SerializeObject(obj);
            Assert.IsNotNull(jsonObj);
        }

        [Test]
        public void PlanSerialization_ExpectedStepFilled()
        {
            var obj = new BaseFarmerPlan("id", "name");

            FarmerPlanStepProvider.Instance.AddFarmerService(new FarmerPlanStep(obj.ID + "_1", new MockedCumulativeTask()));
            obj.EditableSteps.Add(FarmerPlanStepProvider.Instance.GetFarmerService(obj.ID + "_1"));

            var jsonObj = JsonConvert.SerializeObject(obj);
            Assert.IsNotNull(jsonObj);

            var deserializedObj = JsonConvert.DeserializeObject<TestableFarmerPlan>(jsonObj);
            Assert.IsNotNull(deserializedObj);
            Assert.AreEqual(obj.ID, deserializedObj.ID);
            Assert.AreEqual(obj.Name, deserializedObj.Name);
            Assert.AreEqual(obj.Steps, deserializedObj.Steps);
            Assert.IsNull(deserializedObj.LastException);
            Assert.IsFalse(deserializedObj.IsInProgress);
        }

        [Test]
        public void GroundSerialization_ExpectedNotNull()
        {
            var jsonGround = JsonConvert.SerializeObject(_ground);
            Assert.IsNotNull(jsonGround);
        }

        [Test]
        public void GroundDeserialization_ExpectedSame()
        {
            var jsonGround = JsonConvert.SerializeObject(_ground);
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
        
        [Test]
        public void GroundDeserializationWithPlans_ExpectedSame()
        {
            var planId = FarmerPlanProvider.Instance.GenerateServiceId();
            var plan = new BaseFarmerPlan(planId, "test4ground");

            var stepCount = 4;
            var stepIds = new List<string>();
            for (int i=0; i<stepCount; i++)
            {
                var planStepId = FarmerPlanStepProvider.Instance.GenerateServiceId();
                stepIds.Add(planStepId);

                FarmerPlanStepProvider.Instance.AddFarmerService(
                    new FarmerPlanStep(
                        planStepId, 
                        typeof(MockFarmerLeafDetector).FullName,
                        new object[] { i }));
            }

            for (int i=0; i<stepCount; i++)
            {
                plan.EditableSteps.Add(FarmerPlanStepProvider.Instance.GetFarmerService(stepIds[i]));
            }

            FarmerPlanProvider.Instance.AddFarmerService(plan);
            _ground.AddPlan(planId);

            var jsonGround = JsonConvert.SerializeObject(_ground);
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
                var gatheredPlan = FarmerPlanProvider.Instance.GetFarmerService(_ground.PlanIds[i]);
                Assert.AreEqual(gatheredPlan.ID, _ground.PlanIds[i]);
            }

            Assert.AreEqual(_ground.PlanIds.Count, deserializedGround.PlanIds.Count);
        }

        [Test]
        public void GroundDeserializationWithAlerts_ExpectedSame()
        {
            var receivedAlerts = new List<IFarmerAlert>();
            var alertCount = 4;
            var alertIds = new List<string>();
            var alertHandler = FarmerAlertHandler.Instance;

            alertHandler.NewAlertCreated += (s,e) =>
                {
                    receivedAlerts.Add(FarmerAlertProvider.Instance.GetFarmerService(e.AlertId));
                };

            for (int i=0; i<alertCount; i++)
            {
                alertHandler.RaiseAlert("message " + i, null, null, null, AlertLevel.Warning, AlertSeverity.Low);
            }

            Assert.AreEqual(receivedAlerts.Count, _ground.AlertIds.Count);

            var jsonGround = JsonConvert.SerializeObject(_ground);
            Assert.IsNotNull(jsonGround);

            var deserializedGround = JsonConvert.DeserializeObject<FarmerGround>(jsonGround);

            Assert.IsNotNull(deserializedGround);
            Assert.AreEqual(_ground.ID, deserializedGround.ID);
            Assert.AreEqual(_ground.GroundName, deserializedGround.GroundName);

            for (int i = 0; i < _ground.AlertIds.Count; i++)
            {
                var alert = receivedAlerts.FirstOrDefault(x => x.ID == _ground.AlertIds[i]);

                Assert.IsNotNull(alert);
 
                var storedAlert = FarmerAlertProvider.Instance.GetFarmerService(alert.ID);

                Assert.AreEqual(alert.ID, storedAlert.ID);
                Assert.AreEqual(alert.Code, storedAlert.Code);
                Assert.AreEqual(alert.Level, storedAlert.Level);
                Assert.AreEqual(alert.Severity, storedAlert.Severity);
                Assert.AreEqual(alert.MarkedAsRead, storedAlert.MarkedAsRead);
            }
        }    
    }
}