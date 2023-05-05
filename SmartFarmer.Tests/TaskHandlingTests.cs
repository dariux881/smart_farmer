using System;
using NUnit.Framework;
using SmartFarmer.Exceptions;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks.Health;
using SmartFarmer.Utils;

namespace SmartFarmer.Tests
{
    [TestFixture]
    public class TaskHandlingTests
    {
        [Test]
        public void LookingForTaskByType_ExpectedSuccess()
        {
            var type = typeof(IFarmerLeavesStatusCheckerTask);

            var task = FarmerTaskProvider.GetTaskDelegateByType(type);

            Assert.IsNotNull(task);
            Assert.IsTrue(task is IFarmerLeavesStatusCheckerTask);
        }

        [Test]
        public void ReloadingSameTask_ExpectedSameID()
        {
            var type = typeof(IFarmerParasiteCheckerTask);

            var task1 = FarmerTaskProvider.GetTaskDelegateByType(type);
            var task2 = FarmerTaskProvider.GetTaskDelegateByType(type);

            Assert.IsNotNull(task1);
            Assert.IsNotNull(task2);
            Assert.AreEqual(task1.ID, task2.ID);
        }

        [Test]
        public void LoadingDifferentTasks_ExpectedSameClassDifferentID()
        {
            var type1 = typeof(IFarmerLeavesStatusCheckerTask);
            var type2 = typeof(IFarmerParasiteCheckerTask);

            var task1 = FarmerTaskProvider.GetTaskDelegateByType(type1);
            var task2 = FarmerTaskProvider.GetTaskDelegateByType(type2);

            Assert.IsNotNull(task1);
            Assert.IsNotNull(task2);
            Assert.AreNotEqual(task1.ID, task2.ID);
        }

        [Test]
        public void LookingForTaskByTypeInExcludedNamespace_ExpectedNull()
        {
            var type = typeof(IFarmerParasiteCheckerTask);

            IFarmerTask task = null;
            Exception foundEx = null;

            try {
                task = FarmerTaskProvider.GetTaskDelegateByType(
                    type, 
                    new [] {"SmartFarmer.MockedTasks.GenericCollection"});
            }
            catch (Exception ex)
            {
                foundEx = ex;
            }

            Assert.IsNull(task);
            Assert.IsNotNull(foundEx);
            Assert.IsTrue(foundEx is TaskNotFoundException);
        }
    }
}