using NUnit.Framework;
using SmartFarmer.Alerts;
using SmartFarmer.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace SmartFarmer.Tests
{
    [TestFixture]
    public class AlertHandlingTests
    {
        private IFarmerGround _ground;

        public AlertHandlingTests() 
        {
            _ground = new FarmerGround(
                FarmerPlantProvider.Instance, 
                FarmerIrrigationInfoProvider.Instance, 
                FarmerPlantInstanceProvider.Instance, 
                FarmerPlanProvider.Instance,
                FarmerAlertProvider.Instance, 
                FarmerAlertHandler.Instance);
        }

        [Test]
        public async Task RaisingAlert_ExpectedGroundFound()
        {
            var alertHandler = FarmerAlertHandler.Instance;

            Assert.IsNotNull(_ground);
            Assert.IsNotNull(alertHandler);
            Assert.IsEmpty(_ground.AlertIds);

            var message = "test";

            await alertHandler.RaiseAlert(message, AlertCode.Unknown, null, null, AlertLevel.Error, AlertSeverity.Low);

            Assert.IsNotEmpty(_ground.AlertIds);

            foreach (var alertId in _ground.AlertIds)
            {
                var receivedAlert = await FarmerAlertProvider.Instance.GetFarmerService(alertId);
                Assert.IsNotNull(receivedAlert);
                Assert.AreEqual(message, receivedAlert.Message);
            }
        }
    }
}