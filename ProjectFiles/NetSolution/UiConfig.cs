using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAManagedCore;
using FTOptix.HMIProject;
using FTOptix.Core;
using FTOptix.NetLogic;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.Store;

public class UiConfig : BaseNetLogic
{
    public Label lblSelectedValue;
    public readonly string modelVarSelectedValueStr = "Model/Monitor/CPUTempInst";
    public readonly string modelVarSelectedValueAvgStr = "Model/Monitor/CPUTempAvg";
    public readonly string modelFanStateColorStr = "Model/Monitor/FANStateColor";
    public readonly string modelFanOnLimitStr = "Model/Monitor/FANOnLimit";
    public readonly string modelFanOffLimitStr = "Model/Monitor/FANOffLimit";
    public readonly string fanStateOnColor = "4294901760";  //#FFFF0000 (RED)
    public readonly string fanStateOffColor = "4278255360"; // #FF00FF00 (GREEN)
    
}