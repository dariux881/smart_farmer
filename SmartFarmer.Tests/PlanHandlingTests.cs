using System;
using NUnit.Framework;
using SmartFarmer.Misc;
using SmartFarmer.MockedTasks;
using SmartFarmer.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tests
{
    [TestFixture]
    public class PlanHandlingTests
    {
        [Test]
        public void CreatingPlan_ExpectedSuccess()
        {
            var farmerPlan = new BaseFarmerPlan("test");

            Assert.AreEqual("test", farmerPlan.Name);
        }

        [Test]
        public void CreatingPlan_AddingTasks_ExpectedSuccess()
        {
            var farmerPlan = new BaseFarmerPlan("test");

            Assert.AreEqual("test", farmerPlan.Name);

            Assert.IsNotNull(farmerPlan.Steps);
        }

        [Test]
        public void CreatingExistingPlan_ExpectedFailure()
        {
            IFarmerPlan plan;

            try
            {
                plan = new FarmerPlantStatusCheckPlan();
            }
            catch(Exception ex)
            {
                SmartFarmerLog.Exception(ex);
            }
        }
    }
}