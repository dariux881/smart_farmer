using NUnit.Framework;
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
            var type = typeof(IFarmerLeavesStatusChecker);

            var task = FarmerTaskProvider.GetTaskDelegateByType(type);

            Assert.IsNotNull(task);
            Assert.IsTrue(task is MockedLeavesStatusChecker);
        }

        [Test]
        public void ReloadingSameTask_ExpectedSameID()
        {
            var type = typeof(IFarmerLeavesStatusChecker);

            var task1 = FarmerTaskProvider.GetTaskDelegateByType(type) as MockedLeavesStatusChecker;
            var task2 = FarmerTaskProvider.GetTaskDelegateByType(type) as MockedLeavesStatusChecker;

            Assert.IsNotNull(task1);
            Assert.IsNotNull(task2);
            Assert.AreEqual(task1.ID, task2.ID);
        }

        [Test]
        public void LoadingDifferentTasks_ExpectedSameClassDifferentID()
        {
            var type1 = typeof(IFarmerLeavesStatusChecker);
            var type2 = typeof(IFarmerParasiteChecker);

            var task1 = FarmerTaskProvider.GetTaskDelegateByType(type1) as MockedLeavesStatusChecker;
            var task2 = FarmerTaskProvider.GetTaskDelegateByType(type2) as MockedLeavesStatusChecker;

            Assert.IsNotNull(task1);
            Assert.IsNotNull(task2);
            Assert.AreNotEqual(task1.ID, task2.ID);
        }
    }
}