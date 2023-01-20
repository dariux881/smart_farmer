using System;

namespace SmartFarmer.Settings
{
    public class UserDefinedSettings
    {
        private bool? _autoIrrigation_autoStart;
        private DateTime? _autoIrrigation_plannedTime;

        public bool AUTOIRRIGATION_AUTOSTART => 
            _autoIrrigation_autoStart ?? 
            GlobalSettings.DEFAULT_AUTOIRRIGATION_AUTOSTART;

        public DateTime AUTOIRRIGATION_PLANNED_TIME =>
            _autoIrrigation_plannedTime ??
            GlobalSettings.DEFAULT_AUTOIRRIGATION_PLANNED_TIME;

        public void SetAutoIrrigationAutoStart(bool value)
        {
            _autoIrrigation_autoStart = value;
        }

        public void SetAutoIrrigationPlannedTime(DateTime value)
        {
            _autoIrrigation_plannedTime = value;
        }
    }
}