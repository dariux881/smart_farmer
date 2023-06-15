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
        private IFarmerGarden _garden;

        public AlertHandlingTests() 
        {
            _garden = new FarmerGarden(
                FarmerPlantProvider.Instance, 
                FarmerIrrigationInfoProvider.Instance, 
                FarmerPlantInstanceProvider.Instance, 
                FarmerPlanProvider.Instance,
                FarmerAlertProvider.Instance, 
                FarmerAlertHandler.Instance);
        }

        [Test]
        public async Task RaisingAlert_ExpectedGardenFound()
        {
            var alertHandler = FarmerAlertHandler.Instance;

            Assert.IsNotNull(_garden);
            Assert.IsNotNull(alertHandler);
            Assert.IsEmpty(_garden.AlertIds);

            var message = "test";

            await alertHandler.RaiseAlert(message, AlertCode.Unknown, null, null, null, AlertLevel.Error, AlertSeverity.Low);

            Assert.IsNotEmpty(_garden.AlertIds);

            foreach (var alertId in _garden.AlertIds)
            {
                var receivedAlert = await FarmerAlertProvider.Instance.GetFarmerService(alertId);
                Assert.IsNotNull(receivedAlert);
                Assert.AreEqual(message, receivedAlert.Message);
            }
        }
    }
}