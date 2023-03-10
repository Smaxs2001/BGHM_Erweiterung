using System;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Streaming;

namespace BFE.HelpersX
{
    public class GenericStringEventArgs : EventArgs
    {
        public GenericStringEventArgs(string arg)
        {
            StringData = arg;
        }
        public string StringData { get; }
    }

    public class GenericBoolEventArgs : EventArgs
    {
        public GenericBoolEventArgs(bool arg)
        {
            BoolData = arg;
        }
        public bool BoolData { get; }
    }

    public class GenericIntegerEventArgs : EventArgs
    {
        public GenericIntegerEventArgs(int arg)
        {
            IntData = arg;
        }
        public int IntData { get; }
    }

    public class CrestronEnumEventArgsEHdcpCapabilityType : EventArgs
    {
        public CrestronEnumEventArgsEHdcpCapabilityType(eHdcpCapabilityType arg)
        {
            EnumData = arg;
        }
        public eHdcpCapabilityType EnumData { get; }
    }

    public class CrestronEnumEventArgsESfpVideoSourceTypes : EventArgs
    {
        public CrestronEnumEventArgsESfpVideoSourceTypes(eSfpVideoSourceTypes arg)
        {
            EnumData = arg;
        }
        
        public eSfpVideoSourceTypes EnumData { get; }
    }
}