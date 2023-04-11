#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.DataLogger;
using FTOptix.NetLogic;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.Store;
using FTOptix.SQLiteStore;
using FTOptix.OPCUAServer;
using FTOptix.Modbus;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.Alarm;
using FTOptix.CommunicationDriver;
using FTOptix.Core;
using FTOptix.EventLogger;
#endregion
public class OptixMiscFunctions
{
    private ProjectFolder project_current;
    public OptixMiscFunctions(ProjectFolder project_current)
    {
        this.project_current = project_current;
    }
    public Label GetLblObjectFromId(IUAObject logicObject, NodeId lblNodeId)
    {
        Label lbl = (Label) logicObject.Context.GetObject(lblNodeId);
        if (lbl == null)
        {
            throw new Exception("GetTbxObjectFromId - Label with Id: " + lblNodeId.ToString() + " was not found");
        }
        return lbl;
    }

    public Label GetLblObjectFromName(string lblName)
    {
        Label lbl = (Label) project_current.GetObject(lblName);
        if (lbl == null)
        {
            throw new Exception("GetTbxObjectFromName - Label with name: " + lblName + " was not found");
        }
        return lbl;
    }

    public TextBox GetTbxObjectFromName(string tbxName)
    {
        TextBox tbx = (TextBox) project_current.GetObject(tbxName);
        if (tbx == null)
        {
            throw new Exception("GetTbxObjectFromName - TextBox with name: " + tbxName + " was not found");
        }
        return tbx;
    }
    public void UpdateLblObjectValue(string  lblName, string value)
    {
        Label lbl = GetLblObjectFromName(lblName);
        lbl.Text = value;
    }
    public void UpdateTbxObjectValue(string  tbxName, string value)
    {
        TextBox tbx = GetTbxObjectFromName(tbxName);
        tbx.Text = value;
    }

    public UAValue GetVariableModelValue(string varName)
    {
        IUAVariable variable = Project.Current.GetVariable(varName);
        if (variable == null) 
        {
            throw new Exception("Model Variable "  + varName + " not found.");
        }
        return variable.Value;
    }
    public void UpdateVariableModelValue(string varName, string value)
    {
        IUAVariable variable = Project.Current.GetVariable(varName);
        if (variable == null) 
        {
            throw new Exception("Model Variable "  + varName + " not found.");
        }
        variable.Value = new UAValue(value);
    }

}