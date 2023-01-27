using NUnit.Framework;
using SmartFarmer.Alerts;
using SmartFarmer.Utils;
using System.Linq;

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
        public void RaisingAlert_ExpectedGroundFound()
        {
            var alertHandler = FarmerAlertHandler.Instance;

            Assert.IsNotNull(_ground);
            Assert.IsNotNull(alertHandler);
            Assert.IsEmpty(_ground.AlertIds);

            var message = "test";

            alertHandler.RaiseAlert(message, null, null, null, AlertLevel.Error, AlertSeverity.Low);

            Assert.IsNotEmpty(_ground.AlertIds);
            var alert = 
                _ground
                    .AlertIds
                        .Select(x => FarmerAlertProvider.Instance.GetFarmerService(x))
                        .FirstOrDefault(x => x.Message == message);

            Assert.IsNotNull(alert);
        }
    }
}