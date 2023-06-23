using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SmartFarmer.Misc;
using SmartFarmer.MockedTasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tests
{
    [TestFixture]
    public class PlanHandlingTests
    {
        [Test]
        public void CreatingPlan_ExpectedSuccess()
        {
            var farmerPlan = new BaseFarmerPlan("id", "test");

            Assert.AreEqual("test", farmerPlan.Name);
        }

        [Test]
        public void CreatingPlan_AddingTasks_ExpectedSuccess()
        {
            var farmerPlan = new BaseFarmerPlan("id", "test");

            Assert.AreEqual("test", farmerPlan.Name);

            Assert.IsNotNull(farmerPlan.Steps);
        }

        [Test]
        public void CreatingExistingPlan_MissingImplementors_ExpectedFailure()
        {
            IFarmerPlan plan;

            try
            {
                plan = new FarmerPlantStatusCheckPlan("id");
            }
            catch(Exception ex)
            {
                SmartFarmerLog.Exception(ex);
                Assert.Pass();
            }

            Assert.Fail();
        }

        [Test]
        public async Task RunningPlan_ExistingImplementors_ExpectedSuccess()
        {
            var plan = new BaseFarmerPlan("id", "test");

            var leafDet = new MockFarmerLeafDetector();
            var leafCheck = new MockFarmerLeavesStatusChecker();
            var stem = new MockFarmerStemDetector();

            plan.EditableSteps.Add(new FarmerPlanStep(plan.ID + "_1", leafDet));
            plan.EditableSteps.Add(new FarmerPlanStep(plan.ID + "_2", leafCheck));
            plan.EditableSteps.Add(new FarmerPlanStep(plan.ID + "_3", stem));

            Assert.IsNotNull(plan.Steps);
            Assert.IsNotEmpty(plan.Steps);

            await plan.Execute(System.Threading.CancellationToken.None);
        }

        [Test]
        public async Task RunningPlan_ExistingImplementors_ExpectedFailure()
        {
            var plan = new BaseFarmerPlan("id", "test");

            var leafDet = new MockFarmerLeafDetector();
            var leafCheck = new MockFarmerLeavesStatusChecker();
            var stem = new MockFarmerStemDetector() {ExpectFail = true};

            plan.EditableSteps.Add(new FarmerPlanStep(plan.ID + "_1", leafDet));
            plan.EditableSteps.Add(new FarmerPlanStep(plan.ID + "_2", leafCheck));
            plan.EditableSteps.Add(new FarmerPlanStep(plan.ID + "_3", stem));

            Assert.IsNotNull(plan.Steps);
            Assert.IsNotEmpty(plan.Steps);

            try {
                await plan.Execute(System.Threading.CancellationToken.None);
            }
            catch (Exception) {
                Assert.Pass();
            }
        }    
    }
}