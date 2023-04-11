    
//namespace CPUFanController1
//{
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
        public readonly int monitorPollPeriod = 1000;
        public readonly int averagePeriod = 10000;
    }
//}