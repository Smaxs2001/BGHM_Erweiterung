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
    class NvxRouting
    {
        private DmNvxBaseClass[] _dmNvxTransmitter;
        private DmNvxBaseClass[] _dmNvxReceiver;
        private ControlSystem _controlSystem;
        private ComPort.ComPortSpec _standard;
        private uint[] _routes;
        public NvxRouting(ControlSystem paramControlSystem) {



            // Transmitter E026_ClickShare,E026_Bodentank1,E026_Bodentank2,E026_Bodentank3,E026_Kamera,E002_ClickShare,E002_Bodentank1,E002_Bodentank2
            // receiver E026_beamer1,E026_beamer2,E026_Display_Links,E026_Display_rechts,E026_Display_vorne,E002_Display_Links,E002_Display_rechts,E026-Display_Mitte


            _controlSystem = paramControlSystem;
            _dmNvxTransmitter = new DmNvxBaseClass[8];
            _dmNvxReceiver = new DmNvxBaseClass[8];

            _standard.BaudRate = ComPort.eComBaudRates.ComspecBaudRate9600;
            _standard.Protocol = ComPort.eComProtocolType.ComspecProtocolRS232;
            _standard.Parity = ComPort.eComParityType.ComspecParityNone;
            _standard.DataBits = ComPort.eComDataBits.ComspecDataBits8;
            _standard.StopBits = ComPort.eComStopBits.ComspecStopBits1;

            _routes = new uint[8];

            for (int i = 0; i < 8; i++)
            {
                _dmNvxTransmitter[i] = new DmNvxE30((uint)i+5, _controlSystem);
                _dmNvxTransmitter[i].OnlineStatusChange += new OnlineStatusChangeEventHandler(NvxOnlineStatusChange);
            }
            for (int i = 0; i < 7; i++)
            {
                _dmNvxReceiver[i] = new DmNvx360((uint)i+14, _controlSystem);
            }
            _dmNvxReceiver[7] = new DmNvxD30(21, _controlSystem);


            foreach (DmNvxBaseClass nvxdevice in _dmNvxReceiver)
            {

                if (nvxdevice.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                {

                    ErrorLog.Error("Error in Registering Device : {0}", nvxdevice.RegistrationFailureReason);
                }

                    
                
            }
            foreach (DmNvxBaseClass nvxdevice in _dmNvxTransmitter)
            {
               
                if (nvxdevice.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    ErrorLog.Error("Error in Registering Device : {0}", nvxdevice.RegistrationFailureReason);
                }
            }

        }

        public void Init()
        {
            foreach (DmNvxBaseClass nvxdevice in _dmNvxReceiver)
            {
                {
                    if (nvxdevice.NumberOfComPorts > 0)
                    {
                        foreach(ComPort x in nvxdevice.ComPorts)
                        {
                            x.SetComPortSpec(_standard);
                        }
                    }
                }
            }
        }

        //only change route if route differs from lst value
        public void ChangeRoute(uint source,uint destination)
        {
            if (source >= _dmNvxTransmitter.Length) return;
            if (destination >= _dmNvxReceiver.Length) return;
            if (!_dmNvxTransmitter[source].IsOnline) return;
            if (!_dmNvxReceiver[destination].IsOnline) return;
            if (_dmNvxReceiver[destination].Control.ServerUrlFeedback.StringValue == _dmNvxTransmitter[source].Control.ServerUrlFeedback.StringValue) return;

            if (_dmNvxReceiver[destination].HdmiOut.BlankEnabledFeedback.BoolValue) _dmNvxReceiver[destination].HdmiOut.BlankDisabled();
            _dmNvxReceiver[destination].Control.ServerUrl.StringValue = _dmNvxTransmitter[source].Control.ServerUrlFeedback.StringValue;
            _routes[destination]=source;
            _controlSystem.PanelE026.StringInput[2].StringValue = ((NvxTransmitter)GetSource(destination)).ToString();
        }

        void NvxOnlineStatusChange(GenericBase device, OnlineOfflineEventArgs args)
        {
            if(device.ID > 4 && device.ID < 14 && device.IsOnline)
            {
                _dmNvxTransmitter[device.ID-5].Control.EnableAutomaticInitiation();
                _dmNvxTransmitter[device.ID-5].Control.EnableAutomaticInputRouting();
            }else
            {
                if(device.ID > 13 && device.ID < 22 && device.IsOnline)
                {
                    _dmNvxReceiver[device.ID-14].Control.EnableAutomaticInitiation();
                }
            }

        }

        public void NvxBlack(uint destination)
        {
            _dmNvxReceiver[destination].HdmiOut.BlankEnabled();
            _controlSystem.PanelE026.StringInput[2].StringValue = "Schwarzbild";
        }

        public void Rs232Send(uint destination, string message)
        {
            _dmNvxReceiver[destination].ComPorts[0].Send(message);
        }

        public uint GetSource(uint dest)
        {
            return _routes[dest];
        }

    }
}
