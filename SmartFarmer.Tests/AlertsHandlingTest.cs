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
                FarmerPlantInstanceProvider.Instance, 
                FarmerPlanProvider.Instance, 
                FarmerAlertHandler.Instance);
        }

        [Test]
        public void RaisingAlert_ExpectedGroundFound()
        {
            var alertHandler = FarmerAlertHandler.Instance;

            Assert.IsNotNull(_ground);
            Assert.IsNotNull(alertHandler);
            Assert.IsEmpty(_ground.Alerts);

            var message = "test";

            alertHandler.RaiseAlert(new FarmerAlert()
            {
                Message = message
            });

            Assert.IsNotEmpty(_ground.Alerts);
            var alert = _ground.Alerts.FirstOrDefault(x => x.Message == message);

            Assert.IsNotNull(alert);
        }
    }
}