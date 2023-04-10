#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.UI;
using FTOptix.Retentivity;
using FTOptix.NativeUI;
using FTOptix.Core;
using FTOptix.CoreBase;
using FTOptix.NetLogic;
using LibreHardwareMonitor.Hardware;
#endregion

public class FanControllerLogic : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void Init()
    {
        Monitor();
    }

    public void Monitor()
    {
        Computer computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = false,
            IsMemoryEnabled = false,
            IsMotherboardEnabled = false,
            IsControllerEnabled = false,
            IsNetworkEnabled = false,
            IsStorageEnabled = false
        };

        computer.Open();
        computer.Accept(new UpdateVisitor());

        foreach (IHardware hardware in computer.Hardware)
        {
            Log.Info(string.Format("Hardware: {0}", hardware.Name));
            
            foreach (IHardware subhardware in hardware.SubHardware)
            {
                Log.Info(string.Format("\tSubhardware: {0}", subhardware.Name));
                
                foreach (ISensor sensor in subhardware.Sensors)
                {
                    Log.Info(string.Format("\t\tSensor: {0}, value: {1}", sensor.Name, sensor.Value));
                }
            }

            foreach (ISensor sensor in hardware.Sensors)
            {
                Log.Info(string.Format("\tSensor: {0}, value: {1}", sensor.Name, sensor.Value));
            }
        }
        computer.Close();
    }
}
