using System;

namespace SmartFarmer.Settings
{
    public class UserDefinedSettings : IFarmerSettings
    {
        private bool? _autoIrrigation_autoStart;
        private string _autoIrrigation_cronSchedule;
        private Farmer2dPoint _toolsCollectorPosition;

        public bool AUTOIRRIGATION_AUTOSTART =>
            _autoIrrigation_autoStart ??
            GlobalSettings.DEFAULT_AUTOIRRIGATION_AUTOSTART;

        public Farmer2dPoint TOOLS_COLLECTOR_POSITION =>
            _toolsCollectorPosition ??
            GlobalSettings.TOOLS_COLLECTOR_POSITION;

        public string AUTOIRRIGATION_PLANNED_CRONSCHEDULE =>
            _autoIrrigation_cronSchedule ??
            GlobalSettings.AUTOIRRIGATION_PLANNED_CRONSCHEDULE;

        public void SetAutoIrrigationAutoStart(bool value)
        {
            _autoIrrigation_autoStart = value;
        }

        public void SetAutoIrrigationCronSchedule(string value)
        {
            _autoIrrigation_cronSchedule = value;
        }

        public void SetToolsCollectorPosition(Farmer2dPoint position)
        {
            _toolsCollectorPosition = position;
        }
    }
}