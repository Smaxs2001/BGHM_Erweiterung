using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM.Streaming;
using BGHM_Erweiterung;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BGHM_Erweiterung.ControlSystem;

namespace BFE
{
    class NVXRouting
    {
        private DmNvxBaseClass[] DmNvxTransmitter;
        private DmNvxBaseClass[] DmNvxReceiver;
        private ControlSystem ControlSystem;
        private ComPort.ComPortSpec standard;
        private uint[] routes;
        public NVXRouting(ControlSystem paramControlSystem) {



            // Transmitter E026_ClickShare,E026_Bodentank1,E026_Bodentank2,E026_Bodentank3,E026_Kamera,E002_ClickShare,E002_Bodentank1,E002_Bodentank2
            // receiver E026_beamer1,E026_beamer2,E026_Display_Links,E026_Display_rechts,E026_Display_vorne,E002_Display_Links,E002_Display_rechts,E026-Display_Mitte


            ControlSystem = paramControlSystem;
            DmNvxTransmitter = new DmNvxBaseClass[8];
            DmNvxReceiver = new DmNvxBaseClass[8];

            standard.BaudRate = ComPort.eComBaudRates.ComspecBaudRate9600;
            standard.Protocol = ComPort.eComProtocolType.ComspecProtocolRS232;
            standard.Parity = ComPort.eComParityType.ComspecParityNone;
            standard.DataBits = ComPort.eComDataBits.ComspecDataBits8;
            standard.StopBits = ComPort.eComStopBits.ComspecStopBits1;

            routes = new uint[8];

            for (int i = 0; i < 8; i++)
            {
                DmNvxTransmitter[i] = new DmNvxE30((uint)i+5, ControlSystem);
                DmNvxTransmitter[i].OnlineStatusChange += new OnlineStatusChangeEventHandler(NvxOnlineStatusChange);
            }
            for (int i = 0; i < 7; i++)
            {
                DmNvxReceiver[i] = new DmNvx360((uint)i+14, ControlSystem);
            }
            DmNvxReceiver[7] = new DmNvxD30(21, ControlSystem);


            foreach (DmNvxBaseClass nvxdevice in DmNvxReceiver)
            {

                if (nvxdevice.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                {

                    ErrorLog.Error("Error in Registering Device : {0}", nvxdevice.RegistrationFailureReason);
                }

                    
                
            }
            foreach (DmNvxBaseClass nvxdevice in DmNvxTransmitter)
            {
               
                if (nvxdevice.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    ErrorLog.Error("Error in Registering Device : {0}", nvxdevice.RegistrationFailureReason);
                }
            }

        }

        public void Init()
        {
            foreach (DmNvxBaseClass nvxdevice in DmNvxReceiver)
            {
                {
                    if (nvxdevice.NumberOfComPorts > 0)
                    {
                        foreach(ComPort x in nvxdevice.ComPorts)
                        {
                            x.SetComPortSpec(standard);
                        }
                    }
                }
            }
        }

        //only change route if route differs from lst value
        public void ChangeRoute(uint source,uint destination)
        {
            if (source >= DmNvxTransmitter.Length) return;
            if (destination >= DmNvxReceiver.Length) return;
            if (!DmNvxTransmitter[source].IsOnline) return;
            if (!DmNvxReceiver[destination].IsOnline) return;
            if (DmNvxReceiver[destination].Control.ServerUrlFeedback.StringValue == DmNvxTransmitter[source].Control.ServerUrlFeedback.StringValue) return;

            if (DmNvxReceiver[destination].HdmiOut.BlankEnabledFeedback.BoolValue) DmNvxReceiver[destination].HdmiOut.BlankDisabled();
            DmNvxReceiver[destination].Control.ServerUrl.StringValue = DmNvxTransmitter[source].Control.ServerUrlFeedback.StringValue;
            routes[destination]=source;
            ControlSystem.Panel_E026.StringInput[2].StringValue = ((NvxTransmitter)getSource(destination)).ToString();
        }

        void NvxOnlineStatusChange(GenericBase device, OnlineOfflineEventArgs args)
        {
            if(device.ID > 4 && device.ID < 14 && device.IsOnline)
            {
                DmNvxTransmitter[device.ID-5].Control.EnableAutomaticInitiation();
                DmNvxTransmitter[device.ID-5].Control.EnableAutomaticInputRouting();
            }else
            {
                if(device.ID > 13 && device.ID < 22 && device.IsOnline)
                {
                    DmNvxReceiver[device.ID-14].Control.EnableAutomaticInitiation();
                }
            }

        }

        public void NvxBlack(uint destination)
        {
            DmNvxReceiver[destination].HdmiOut.BlankEnabled();
            ControlSystem.Panel_E026.StringInput[2].StringValue = "Schwarzbild";
        }

        public void RS232Send(uint destination, string Message)
        {
            DmNvxReceiver[destination].ComPorts[0].Send(Message);
        }

        public uint getSource(uint dest)
        {
            return routes[dest];
        }

    }
}
