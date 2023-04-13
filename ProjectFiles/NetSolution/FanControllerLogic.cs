#region Using directives
using System;
using UAManagedCore;
using FTOptix.NetLogic;
using FTOptix.HMIProject;
using System.Reflection;
#endregion

//namespace CPUFanController1
//{
    public class FanControllerLogic : BaseNetLogic
    {
        private Config cfg;
        private UiConfig uicfg;
        private OptixMiscFunctions f;
        private HardwareMonitor monitor;
        private ProjectFolder project_current;
        public override void Start()
        {
            cfg = new Config();
            uicfg = new UiConfig();
            cfg.ReadConfigurationVariables(LogicObject);

            project_current = Project.Current;
            f = new OptixMiscFunctions(project_current);
            
            monitor = new HardwareMonitor(cfg);
        //
        // Open the connection to WMI
        //
        Open();
        // 
        // Now Start the Scan
        //
        StartScan();
        }
        public override void Stop()
        {
            monitor.Dispose();
        }

        [ExportMethod]
        public void Monitor()
        {
            if (monitor.isOpen == false)
            {
                Log.Warning($"{MethodBase.GetCurrentMethod().Name} Hardware Monitor is NOT Open");
                return;
            }
            try
            {
                monitor.Monitor();
            }
            catch (Exception e)
            {
                Log.Error ($"{MethodBase.GetCurrentMethod().Name} Error trying to get Hardware Monitor data. Exception: {e.Message}");
            }
        }

        [ExportMethod]
        public void Open()
        {
            if (monitor.isOpen == true)
            {
                Log.Warning($"{MethodBase.GetCurrentMethod().Name} Hardware Monitor was already Open");
                return;
            }
            try
            {
                monitor.Open();
                Log.Info($"{MethodBase.GetCurrentMethod().Name} Hardware Monitor is Open");
            }
            catch (Exception e)
            {
                Log.Error ($"{MethodBase.GetCurrentMethod().Name} Error trying to open Hardware Monitor. Exception: {e.Message}");
            }
        }


        [ExportMethod]
        public void Close()
        {
            if (monitor.isOpen == false)
            {
                Log.Warning($"{MethodBase.GetCurrentMethod().Name} Hardware Monitor is already Closed");
                return;
            }
            try
            {
                monitor.Close();
                Log.Info($"{MethodBase.GetCurrentMethod().Name} Hardware Monitor is Closed");
            }
            catch (Exception e)
            {
                Log.Error ($"{MethodBase.GetCurrentMethod().Name} Error trying to close Hardware Monitor. Exception: {e.Message}");
            }
        }
        [ExportMethod]
        public void StartScan()
        {
            //
            // Make sure the monitor is opened
            //
            if (monitor.isOpen == false)
            {
                Log.Warning($"{MethodBase.GetCurrentMethod().Name} Hardware Monitor is NOT Open");
                return;
            }
            //
            // Create a Periodic Task
            //
            int pollPeriod = (int) cfg.MinimumPollingPeriodVariable.Value;
            if (pollPeriod < cfg.MinimumPollingPeriod)
            {
                pollPeriod = cfg.MinimumPollingPeriod;
            }
            var scanTask = new PeriodicTask(MonitorScanner, pollPeriod, LogicObject);
            scanTask.Start();
            //MonitorScanner();
        }
        private void MonitorScanner(PeriodicTask task)
        {
            MonitorValues values = new MonitorValues();
            try 
            {
                Monitor();
                //
                // Pick the values from the recently refreshed monitoredDB
                //
                values = monitor.GetSelectedValues(cfg.SelectedSubHardware, cfg.SelectedSensorName, cfg.SelectedSensorType);
            }
            catch (Exception e)
            {
                Log.Error ($"{MethodBase.GetCurrentMethod().Name} Error trying to get new monitor data. Exception: {e.Message}");
            }
            //
            // Put them into Model
            //
            f.UpdateVariableModelValue(uicfg.modelVarSelectedValueStr, values.instantaneous.ToString());
            f.UpdateVariableModelValue(uicfg.modelVarSelectedValueAvgStr, values.average.ToString());
            //
            // Detect Alarms and take actions
            // Get upper and lower limits
            //
            float upperLimit = f.GetVariableModelValue(uicfg.modelFanOnLimitStr);
            float lowerLimit = f.GetVariableModelValue(uicfg.modelFanOffLimitStr);
            if (lowerLimit > upperLimit) 
            {
                Log.Warning("There's a configuration error - upper limit cannot be lower than lower limit.");
            }
            else
            {
                HardwareMonitor.FANState state = monitor.DetectAlarmLimits(cfg.SelectedSubHardware, cfg.SelectedSensorName, cfg.SelectedSensorType, upperLimit, lowerLimit);
                Log.Info("FAN STATE: " + state.ToString());
                //
                // Change color of the LED according to change
                //
                switch (state)
                {
                    case HardwareMonitor.FANState.on:
                        f.UpdateVariableModelValue(uicfg.modelFanStateColorStr, uicfg.fanStateOnColor);
                        f.UpdateVariableModelValue(uicfg.modelFanStateStr, "1");
                        break;
                    case HardwareMonitor.FANState.off:
                        f.UpdateVariableModelValue(uicfg.modelFanStateColorStr, uicfg.fanStateOffColor);
                        f.UpdateVariableModelValue(uicfg.modelFanStateStr, "0");
                        break;
                }
            }
        }
    }   

    
//}
