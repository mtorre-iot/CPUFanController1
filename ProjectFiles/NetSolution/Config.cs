
//namespace CPUFanController1
//{
using UAManagedCore;

public class Config
    {
        //
        // Data Table Constants
        //
        public readonly string TimestampColumnName = "Timestamp";
        public readonly string IndexColumnName = "Index";
        public readonly string HardwareColumnName = "Hardware";
        public readonly string SubHardwareColumnName = "SubHardware";
        public readonly string SensorNameColumnName = "SensorName";
        public readonly string SensorTypeColumnName = "SensorType";
        public readonly string SensorValueColumnName = "SensorValue";
        public readonly string SensorAverageValueColumnName = "SensorAvgValue";
        //
        // 
        public readonly string DeafultSubHardwareName = "<none>";
        public readonly float DefaultNullValue = 0.0f;
        //
        // Selected value
        //
        public readonly string SelectedSubHardware = "<none>";
        public readonly string SelectedSensorName = "CPU Package";
        public readonly string SelectedSensorType = "Temperature";
        //
        // Scanner config
        //
        public readonly string MonitorPollPeriodStr = "PollingPeriod";
        public readonly string AverageWindowStr = "AverageWindow";

        public readonly int MinimumPollingPeriod = 1000;
        public readonly int MinimumAverageWindow = 1000;
        public IUAVariable MinimumPollingPeriodVariable;
        public IUAVariable MinimumAverageWindowVariable;


        public void ReadConfigurationVariables(IUAObject logicObject)
        {
            MinimumPollingPeriodVariable = logicObject.GetVariable(MonitorPollPeriodStr);
            if (MinimumPollingPeriodVariable == null)
            {
                throw new CoreConfigurationException("Minimum Polling Period variable not found");
            }
            MinimumAverageWindowVariable = logicObject.GetVariable(AverageWindowStr);
            if (MinimumAverageWindowVariable == null)
            {
                throw new CoreConfigurationException("Average Window variable not found");
            }
        }
    }
//}