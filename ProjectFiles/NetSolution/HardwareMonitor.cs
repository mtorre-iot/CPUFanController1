#region Using directives
using System;
using System.Data;
using UAManagedCore;
using LibreHardwareMonitor.Hardware;
#endregion

//namespace CPUFanController1
//{
    public class MonitorValues
    {
        public float instantaneous;
        public float average;
        public MonitorValues ()
        {
        this.instantaneous = 0.0f;
        this.average = 0.0f;   
        }

        public MonitorValues (float inst, float avg)
        {
            this.instantaneous = inst;
            this.average = avg;
        }
    }

    public class HardwareMonitor: IDisposable
    {
        public bool isOpen = false;
        private Computer computer;
        private Config cfg;
        public DataTable monitoredDB;

        public HardwareMonitor(Config cfg)
        {
            this.cfg = cfg;
            monitoredDB = new DataTable();
            InitializeDB();
            computer = new Computer
                {
                    IsCpuEnabled = true,
                    IsGpuEnabled = false,
                    IsMemoryEnabled = false,
                    IsMotherboardEnabled = false,
                    IsControllerEnabled = false,
                    IsNetworkEnabled = false,
                    IsStorageEnabled = false
                };
        }
        private void InitializeDB()
        {
            monitoredDB.Columns.Add(cfg.TimestampColumnName, typeof(DateTime));
            monitoredDB.Columns.Add(cfg.IndexColumnName, typeof(string));
            monitoredDB.Columns.Add(cfg.HardwareColumnName, typeof(string));
            monitoredDB.Columns.Add(cfg.SubHardwareColumnName, typeof(string));
            monitoredDB.Columns.Add(cfg.SensorNameColumnName, typeof(string));
            monitoredDB.Columns.Add(cfg.SensorTypeColumnName, typeof(string));
            monitoredDB.Columns.Add(cfg.SensorValueColumnName, typeof(float));
            monitoredDB.Columns.Add(cfg.SensorAverageValueColumnName, typeof(float));        
        }
        public void Open()
        {
            if (isOpen == false) 
            {
                computer.Open();
                isOpen = true;
            }
        }

        public void Close()
        {
            if (isOpen == true) 
            {
                if (computer != null)
                {
                    computer.Close();
                    isOpen = false;
                }
            }
        }
        public void Monitor()
        {   
            if (isOpen == true)
            {
                computer.Accept(new UpdateVisitor());
                foreach (IHardware hardware in computer.Hardware)
                {
                    Log.Info(string.Format("Hardware: {0}", hardware.Name));
                    
                    foreach (IHardware subhardware in hardware.SubHardware)
                    {
                        Log.Info(string.Format("\tSubhardware: {0}", subhardware.Name));
                        
                        foreach (ISensor sensor in subhardware.Sensors)
                        {
                            Log.Info(string.Format("\t\tSensor: {0}, type {1}, value: {2}", sensor.Name, sensor.SensorType, sensor.Value));
                            MonitorValues mv = CalculateAverage(subhardware.Name, sensor.Name, sensor.SensorType.ToString(), sensor.Value);
                            AddOrUpdateTable(hardware.Name, subhardware.Name, sensor.Name, sensor.SensorType.ToString(), mv.instantaneous, mv.average);
                        }
                    }

                    foreach (ISensor sensor in hardware.Sensors)
                    {
                        Log.Info(string.Format("\tSensor: {0}, type {1},  value: {2}", sensor.Name, sensor.SensorType, sensor.Value));
                        MonitorValues mv = CalculateAverage(cfg.DeafultSubHardwareName, sensor.Name, sensor.SensorType.ToString(), sensor.Value);
                        AddOrUpdateTable(hardware.Name, cfg.DeafultSubHardwareName, sensor.Name, sensor.SensorType.ToString(), mv.instantaneous, mv.average);
                    }
                }
                Log.Info(string.Format("Total Rows in DB: {0}", monitoredDB.Rows.Count));
            }
        }

        private MonitorValues CalculateAverage(string subHardware, string sensorName, string sensorType, float? value)
        {
            //
            // get previous value 
            //
            float avg = 0.0f;
            float inst = cfg.DefaultNullValue;
            if (value != null)
            {
                inst = (float) value;
            }

            MonitorValues prev = GetSelectedValues(subHardware, sensorName, sensorType);
            if (prev == null)
            {
                avg = inst;
            }
            else
            {
                float ratio = Convert.ToSingle(cfg.averagePeriod)/Convert.ToSingle(cfg.monitorPollPeriod);
                if (ratio < 1.0f) ratio = 1.0f;
                avg = (prev.average * (ratio-1.0f) + inst) / ratio;
            }
            return new MonitorValues(inst, avg);
        }

        public MonitorValues GetSelectedValues(string subHardware, string sensorName, string sensorType)
        {
            //
            // Do the query to the DB (monitoredDB)
            //
            MonitorValues rtn = new MonitorValues();
            string query = cfg.SubHardwareColumnName + " = '" + subHardware + "' AND ";
            query += cfg.SensorNameColumnName + " = '" + sensorName + "' AND ";
            query += cfg.SensorTypeColumnName + " = '" + sensorType + "'";
            DataRow[] dr = monitoredDB.Select(query);
            if (dr.Length == 0)
            {
                Log.Info("No previous value found in DB");
                return null;
            }
            else
            {
                rtn.instantaneous = (float) dr[0][cfg.SensorValueColumnName];
                rtn.average = (float) dr[0][cfg.SensorAverageValueColumnName];
            }
            Log.Info("Selected Value is: " + rtn.instantaneous.ToString() + " Avg: " + rtn.average.ToString());
            return rtn;

        }
        private void AddOrUpdateTable(string hw, string shw, string sn, string st, float inst, float avg)
        {
            string index = (hw + "-"  + shw + "-" + sn + "-" + st).GetHashCode().ToString("X8");

            string idx = cfg.IndexColumnName + " = '"+ index + "'";
            DataRow[] dr = monitoredDB.Select(idx); 
            if (dr.Length == 0)
            {
                monitoredDB.Rows.Add(DateTime.Now, index, hw, shw, sn, st, inst, avg);
            }
            else if(dr.Length == 1)
            {
                dr[0][cfg.IndexColumnName] = index;
                dr[0][cfg.SensorValueColumnName] = inst;            
                dr[0][cfg.SensorAverageValueColumnName] = avg;            

            }
            else 
            {
                throw new Exception ("There's an error storing data into monitoredDB. They cannot have two identical indexes");
            }
        }

        public enum FANState 
        {
            noChange = -1,
            off = 0,
            on = 1
        }

        public FANState DetectAlarmLimits(string shw, string sn, string st, float upperLimit, float lowerLimit)
        {
            //
            // get current of selected value
            //
            MonitorValues curval = GetSelectedValues(shw, sn, st);
            if (curval == null)
            {
                Log.Warning("No comparison can be done since there's not data yet at DB.");
                return FANState.noChange;
            }
            //
            // Compare limits
            //
            if (curval.average >= upperLimit)
            {
                return FANState.on;
            }
            else if (curval.average <= lowerLimit)
            {
                return FANState.off;
            }
            return FANState.noChange;
        }
        
        public void Dispose()
        {
            Close();
        }
    }
//}