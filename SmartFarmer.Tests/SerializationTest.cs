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
        private IFarmerGarden _garden;

        private class TestableFarmerTask : IFarmerTask
        {
            public FarmerTool RequiredTool { get; set; }
            public string TaskName { get; }

            public bool IsInProgress { get; set; }

            public Exception LastException { get; set; }

            public string ID { get; set; }

            public async Task Execute(CancellationToken token)
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
                        var step = FarmerPlanStepProvider.Instance.GetFarmerService(stepId).Result;
                        if (step == null) throw new InvalidDataException(stepId + " is not a valid step id");

                        _steps.Add(step);
                    }
                }
            }

            public string Name { get; set; }

            public int Priority { get; set; }
            public DateTime? ValidFromDt { get; set; }
            public DateTime? ValidToDt { get; set; }

            public string CronSchedule { get; set; }

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
            _garden = new FarmerGarden(
                "test_garden_id",
                "garden_name",
                10.0,
                11.0,
                10,
                20,
                null,
                "user1",
                null,
                null,
                null,
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
        public void PlantSerialization_ExpectedNotEmpty()
        {
            var plant = FarmerPlantInstanceProvider.Instance.GetFarmerService(_garden.PlantIds.First());

            var jsonPlant = JsonConvert.SerializeObject(plant);
            Assert.IsNotNull(jsonPlant);
        }

        [Test]
        public async Task PlantDeserialization_ExpectedSame()
        {
            var plant = await FarmerPlantInstanceProvider.Instance.GetFarmerService(_garden.PlantIds.Last());

            var jsonPlant = JsonConvert.SerializeObject(plant);
            Assert.IsNotNull(jsonPlant);

            var deserializedPlant = JsonConvert.DeserializeObject<FarmerPlantInstance>(jsonPlant);

            Assert.IsNotNull(deserializedPlant);
            Assert.AreEqual(plant.ID, deserializedPlant.ID);
            Assert.AreEqual(plant.PlantName, deserializedPlant.PlantName);

            Assert.AreEqual(plant.PlantKindID, deserializedPlant.PlantKindID);
            
            var gatheredPlant = await FarmerPlantProvider.Instance.GetFarmerService(plant.PlantKindID);
            Assert.IsNotNull(gatheredPlant);
            Assert.AreEqual(gatheredPlant.ID, deserializedPlant.PlantKindID);
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
            var obj = new FarmerPlanStep("id", typeof(MockedCumulativeTask).FullName, null)
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
        public async Task PlanSerialization_ExpectedStepFilled()
        {
            var obj = new BaseFarmerPlan("id", "name");

            await FarmerPlanStepProvider.Instance.AddFarmerService(new FarmerPlanStep(obj.ID + "_1", new MockedCumulativeTask()));
            obj.EditableSteps.Add(await FarmerPlanStepProvider.Instance.GetFarmerService(obj.ID + "_1"));

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
        public void GardenSerialization_ExpectedNotNull()
        {
            var jsonGarden = JsonConvert.SerializeObject(_garden);
            Assert.IsNotNull(jsonGarden);
        }

        [Test]
        public async Task GardenDeserialization_ExpectedSame()
        {
            var jsonGarden = JsonConvert.SerializeObject(_garden);
            Assert.IsNotNull(jsonGarden);

            var deserializedGarden = JsonConvert.DeserializeObject<FarmerGarden>(jsonGarden);

            Assert.IsNotNull(deserializedGarden);
            Assert.AreEqual(_garden.ID, deserializedGarden.ID);
            Assert.AreEqual(_garden.GardenName, deserializedGarden.GardenName);
            Assert.AreEqual(_garden.Latitude, deserializedGarden.Latitude);
            Assert.AreEqual(_garden.Longitude, deserializedGarden.Longitude);
            Assert.AreEqual(_garden.WidthInMeters, deserializedGarden.WidthInMeters);
            Assert.AreEqual(_garden.LengthInMeters, deserializedGarden.LengthInMeters);
            Assert.AreEqual(_garden.UserID, deserializedGarden.UserID);
            Assert.AreEqual(_garden.PlantIds.Count, deserializedGarden.Plants.Count);
            Assert.AreEqual(_garden.PlanIds.Count, deserializedGarden.Plans.Count);

            for (int i = 0; i < _garden.PlantIds.Count; i++)
            {
                IFarmerPlantInstance plant = await FarmerPlantInstanceProvider.Instance.GetFarmerService(_garden.PlantIds[i]);
                Assert.AreEqual(plant.ID, _garden.PlantIds[i]);
            }

            for (int i = 0; i < _garden.PlanIds.Count; i++)
            {
                var plan = await FarmerPlanProvider.Instance.GetFarmerService(_garden.PlanIds[i]);
                Assert.AreEqual(plan.ID, _garden.PlanIds[i]);
            }

            Assert.AreEqual(_garden.PlanIds.Count, deserializedGarden.PlanIds.Count);
        }
        
        // [Test]
        // public async Task GardenDeserializationWithPlans_ExpectedSame()
        // {
        //     var planId = FarmerPlanProvider.Instance.GenerateServiceId();
        //     var plan = new BaseFarmerPlan(planId, "test4garden");

        //     var stepCount = 4;
        //     var stepIds = new List<string>();
        //     for (int i=0; i<stepCount; i++)
        //     {
        //         var planStepId = FarmerPlanStepProvider.Instance.GenerateServiceId();
        //         stepIds.Add(planStepId);

        //         await FarmerPlanStepProvider.Instance.AddFarmerService(
        //             new FarmerPlanStep(
        //                 planStepId, 
        //                 typeof(MockFarmerLeafDetector).FullName,
        //                 new object[] { i }));
        //     }

        //     for (int i=0; i<stepCount; i++)
        //     {
        //         plan.EditableSteps.Add(await FarmerPlanStepProvider.Instance.GetFarmerService(stepIds[i]));
        //     }

        //     await FarmerPlanProvider.Instance.AddFarmerService(plan);
        //     _garden.AddPlan(planId);

        //     var jsonGarden = JsonConvert.SerializeObject(_garden);
        //     Assert.IsNotNull(jsonGarden);

        //     var deserializedGarden = JsonConvert.DeserializeObject<FarmerGarden>(jsonGarden);

        //     Assert.IsNotNull(deserializedGarden);
        //     Assert.AreEqual(_garden.ID, deserializedGarden.ID);
        //     Assert.AreEqual(_garden.GardenName, deserializedGarden.GardenName);
        //     Assert.AreEqual(_garden.Latitude, deserializedGarden.Latitude);
        //     Assert.AreEqual(_garden.Longitude, deserializedGarden.Longitude);
        //     Assert.AreEqual(_garden.WidthInMeters, deserializedGarden.WidthInMeters);
        //     Assert.AreEqual(_garden.LengthInMeters, deserializedGarden.LengthInMeters);
        //     Assert.AreEqual(_garden.UserID, deserializedGarden.UserID);
        //     Assert.AreEqual(_garden.PlantIds.Count, deserializedGarden.Plants.Count);
        //     Assert.AreEqual(_garden.PlanIds.Count, deserializedGarden.Plans.Count);

        //     for (int i = 0; i < _garden.PlantIds.Count; i++)
        //     {
        //         IFarmerPlantInstance plant = await FarmerPlantInstanceProvider.Instance.GetFarmerService(_garden.PlantIds[i]);
        //         Assert.AreEqual(plant.ID, _garden.PlantIds[i]);
        //     }

        //     for (int i = 0; i < _garden.PlanIds.Count; i++)
        //     {
        //         var gatheredPlan = await FarmerPlanProvider.Instance.GetFarmerService(_garden.PlanIds[i]);
        //         Assert.AreEqual(gatheredPlan.ID, _garden.PlanIds[i]);
        //     }

        //     Assert.AreEqual(_garden.PlanIds.Count, deserializedGarden.PlanIds.Count);
        // }

        [Test]
        public async Task GardenDeserializationWithAlerts_ExpectedSame()
        {
            var receivedAlerts = new List<IFarmerAlert>();
            var alertCount = 4;
            var alertIds = new List<string>();
            var alertHandler = FarmerAlertHandler.Instance;

            alertHandler.NewAlertCreated += async (s,e) =>
                {
                    receivedAlerts.Add(await FarmerAlertProvider.Instance.GetFarmerService(e.AlertId));
                };

            for (int i=0; i<alertCount; i++)
            {
                await alertHandler.RaiseAlert("message " + i, AlertCode.Unknown, null, null, null, AlertLevel.Warning, AlertSeverity.Low);
            }

            Assert.AreEqual(receivedAlerts.Count, _garden.AlertIds.Count);

            var jsonGarden = JsonConvert.SerializeObject(_garden);
            Assert.IsNotNull(jsonGarden);

            var deserializedGarden = JsonConvert.DeserializeObject<FarmerGarden>(jsonGarden);

            Assert.IsNotNull(deserializedGarden);
            Assert.AreEqual(_garden.ID, deserializedGarden.ID);
            Assert.AreEqual(_garden.GardenName, deserializedGarden.GardenName);

            for (int i = 0; i < _garden.AlertIds.Count; i++)
            {
                var alert = receivedAlerts.FirstOrDefault(x => x.ID == _garden.AlertIds[i]);

                Assert.IsNotNull(alert);
 
                var storedAlert = await FarmerAlertProvider.Instance.GetFarmerService(alert.ID);

                Assert.AreEqual(alert.ID, storedAlert.ID);
                Assert.AreEqual(alert.Code, storedAlert.Code);
                Assert.AreEqual(alert.Level, storedAlert.Level);
                Assert.AreEqual(alert.Severity, storedAlert.Severity);
                Assert.AreEqual(alert.MarkedAsRead, storedAlert.MarkedAsRead);
            }
        }    
    }
}