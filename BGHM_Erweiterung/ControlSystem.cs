using System;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.DeviceSupport;             // For Generic Device Support
using Crestron.SimplSharpPro.UI;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro.Lighting;
using System.Collections.Generic;

using BFE;
using BFE.HelpersX;
using BFE.QSYS;


// Aufgaben und Fragen

//TODO Dieter Fragen: Ansteuerung Displays Mitte (CEC) ? NVX Typ für mitschau? Display ansteuerung ?
//TODO Pascal Fragen: QSC ansteuerung

namespace BGHM_Erweiterung
{
    public class ControlSystem : CrestronControlSystem
    {
        private Interlock _senkenWahl;
        private Interlock _pageSelection;
        public readonly Ts1070 PanelE026;
        public readonly Ts1070 PanelE002;
        private Din2Mc2 Leinwand1 { get; }
        private Din2Mc2 Leinwand2 { get; }
        private NvxRouting _nvxRouter;
        private Passwort _adminAccess;
        private bool _kopplung;
        private Volume _e002Volume;
        private Volume[] _e026Volume;
        private Qsys _dsp;


        private uint _selectedReceiver;
        public ControlSystem()
            : base()
        {
            try
            {

                Thread.MaxNumberOfUserThreads = 20;

                //Subscribe to the controller events (System, Program, and Ethernet)
                CrestronEnvironment.SystemEventHandler += new SystemEventHandler(_ControllerSystemEventHandler);

                CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(_ControllerProgramEventHandler);

                CrestronEnvironment.EthernetEventHandler += new EthernetEventHandler(_ControllerEthernetEventHandler);

                //Create Touchscreens
                PanelE026 = new Ts1070(4, this);
                //Panel_E026.Button

                if (PanelE026.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    ErrorLog.Error("Failure in myPanel registration = {0}", PanelE026.RegistrationFailureReason);
                }

                PanelE002 = new Ts1070(3, this);

                if (PanelE002.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    ErrorLog.Error("Failure in myPanel registration = {0}", PanelE002.RegistrationFailureReason);
                }

                Leinwand1 = new Din2Mc2(3, this);
                Leinwand2 = new Din2Mc2(4, this);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in the constructor: {0}", e.Message);
            }

        }
        private void InitializePanel()
        {
            string sgdFilePath = Path.Combine(Directory.GetApplicationDirectory(), "BGHM_Erweiterung_Groß_V0.1_TS1070.sgd");


            PanelE026.SigChange +=Panel_E026_SigChange;

            if (File.Exists(sgdFilePath))
            {
                // Load the SGD File into the panel definition
                PanelE026.LoadSmartObjects(sgdFilePath);

                foreach (KeyValuePair<uint, SmartObject> pair in PanelE026.SmartObjects)
                {
                    pair.Value.SigChange += MySmartObjectSigChange;
                }
            }
            else
            {
                ErrorLog.Error("SmartGraphics Definition file not found! Set .sgd file to 'Copy Always'!");
            }

            PanelE002.SigChange +=Panel_E002_SigChange;

        }
        private void Panel_E002_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            switch (args.Sig.Type)
            {
                case eSigType.Bool:
                    {
                        switch (args.Sig.Number)
                        {
                            case 7: _nvxRouter.ChangeRoute(5, 5); break;
                            case 8: _nvxRouter.ChangeRoute(5, 6); break;
                            case 9: _nvxRouter.ChangeRoute(5, 5); _nvxRouter.ChangeRoute(5, 6); break;
                            case 10: _nvxRouter.ChangeRoute(6, 5); break;
                            case 11: _nvxRouter.ChangeRoute(6, 6); break;
                            case 12: _nvxRouter.ChangeRoute(6, 5); _nvxRouter.ChangeRoute(6, 6); break;
                            case 13: _nvxRouter.ChangeRoute(7, 5); break;
                            case 14: _nvxRouter.ChangeRoute(7, 6); break;
                            case 15: _nvxRouter.ChangeRoute(7, 5); _nvxRouter.ChangeRoute(7, 6); break;
                            case 16:
                                if (args.Sig.BoolValue) { _e002Volume.VolumeUpStart(); }
                                else { _e002Volume.VolumeUpStop(); }
                                break;
                            case 17: if (args.Sig.BoolValue) _e002Volume.Mute(); break;
                            case 18:
                                if (args.Sig.BoolValue) { _e002Volume.VolumeDownStart(); }
                                else { _e002Volume.VolumeDownStop(); }
                                break;
                            case 20: E002_PowerOff(); break;
                            case 21: E002_PowerOn(); break;

                        }
                        break;
                    }
            }
        }
        private void Panel_E026_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            switch (args.Sig.Type)
            {
                case eSigType.Bool:
                    {
                        if (args.Sig.Number == 12)
                        {
                            if (args.Sig.BoolValue) { _e026Volume[8].VolumeUpStart(); }
                            else { _e026Volume[8].VolumeUpStop(); }
                            break;
                        }
                        if (args.Sig.Number == 11)
                        {
                            if (args.Sig.BoolValue) { _e026Volume[8].Mute(); }
                            break;
                        }
                        if (args.Sig.Number == 10)
                        {
                            if (args.Sig.BoolValue) { _e026Volume[8].VolumeDownStart(); }
                            else { _e026Volume[8].VolumeDownStop(); }
                            break;
                        }

                        if (args.Sig.BoolValue)
                        {
                            if (args.Sig.Number == 7) KopplungChanged();

                            if (args.Sig.Number == 8) _pageSelection.Activate(0);

                            if (args.Sig.Number == 10) E026_PowerOn();

                            if (args.Sig.Number > 15 && args.Sig.Number <21)
                            {
                                _nvxRouter.ChangeRoute(args.Sig.Number-16, _selectedReceiver);
                                if (_kopplung)
                                {
                                    if (_selectedReceiver == 0)
                                    {
                                        _nvxRouter.ChangeRoute(args.Sig.Number-16, 5);
                                    }
                                    if (_selectedReceiver == 1)
                                    {
                                        _nvxRouter.ChangeRoute(args.Sig.Number-16, 6);
                                    }
                                }
                            }
                            if (args.Sig.Number == 21)
                            {
                                _nvxRouter.NvxBlack(_selectedReceiver);
                            }
                            if (args.Sig.Number == 22)
                            {
                                SenkeAn();
                            }
                            if (args.Sig.Number == 23)
                            {
                                SenkeAus();
                            }
                            if (args.Sig.Number == 24)
                            {
                                CrestronConsole.PrintLine("Hallo");
                                _pageSelection.Activate(0);
                            }
                            if (args.Sig.Number == 25)
                            {
                                CrestronConsole.PrintLine("Hallo");
                                _pageSelection.Activate(1);
                            }
                            if (args.Sig.Number == 26)
                            {
                                CrestronConsole.PrintLine("Hallo");
                                _pageSelection.Activate(2);
                            }
                            if(args.Sig.Number == 137) E026_PowerOff();

                        }
                        break;
                    }
            }
        }
        private void MySmartObjectSigChange(GenericBase currentDevice, SmartObjectEventArgs args)
        {
            switch ((PanelSmartObjectIDs)args.SmartObjectArgs.ID)
            {
                #region Tab Button Smartgraphics
                case PanelSmartObjectIDs.TabButtonVertical:
                    {
                        switch (args.Sig.Name)
                        {
                            
                            case ("Tab Button 1 Press"):
                                {
                                    _senkenWahl.Activate(0);
                                    _selectedReceiver = 0;
                                    PanelE026.StringInput[2].StringValue = ((NvxTransmitter)_nvxRouter.GetSource(_selectedReceiver)).ToString();
                                    PanelE026.StringInput[5].StringValue = "Beamer 1 ausgewählt";
                                    break;
                                }
                            case ("Tab Button 2 Press"):
                                {
                                    _senkenWahl.Activate(1);
                                    _selectedReceiver = 1;
                                    PanelE026.StringInput[2].StringValue = ((NvxTransmitter)_nvxRouter.GetSource(_selectedReceiver)).ToString();
                                    PanelE026.StringInput[5].StringValue = "Beamer 2 ausgewählt";
                                    break;
                                }
                            case ("Tab Button 3 Press"):
                                {
                                    _senkenWahl.Activate(2);
                                    _selectedReceiver = 2;
                                    PanelE026.StringInput[2].StringValue = ((NvxTransmitter)_nvxRouter.GetSource(_selectedReceiver)).ToString();
                                    PanelE026.StringInput[5].StringValue = "Display Links ausgewählt";
                                    break;
                                }
                            case ("Tab Button 4 Press"):
                                {
                                    _senkenWahl.Activate(3);
                                    _selectedReceiver = 7;
                                    PanelE026.StringInput[2].StringValue = ((NvxTransmitter)_nvxRouter.GetSource(_selectedReceiver)).ToString();
                                    PanelE026.StringInput[5].StringValue = "Display Mitte ausgewählt";
                                    break;
                                }
                            case ("Tab Button 5 Press"):
                                {
                                    _senkenWahl.Activate(4);
                                    _selectedReceiver = 3;
                                    PanelE026.StringInput[2].StringValue = ((NvxTransmitter)_nvxRouter.GetSource(_selectedReceiver)).ToString();
                                    PanelE026.StringInput[5].StringValue = "Display Rechts ausgewählt";
                                    break;
                                }
                            case ("Tab Button 6 Press"):
                                {
                                    _senkenWahl.Activate(5);
                                    _selectedReceiver = 4;
                                    PanelE026.StringInput[2].StringValue = ((NvxTransmitter)_nvxRouter.GetSource(_selectedReceiver)).ToString();
                                    PanelE026.StringInput[5].StringValue = "Display Vorne ausgewählt";
                                    break;
                                }
                        }
                        break;
                    }
                #endregion

                #region Keypad Smartobject
                case PanelSmartObjectIDs.Keypad:
                    {
                        if (args.Sig.Name == "Misc_1")
                        {
                            _adminAccess.Exit();
                        }
                        if (args.Sig.Name == "Misc_2")
                        {
                            _adminAccess.CheckPassword();
                        }
                        if (args.Sig.BoolValue)
                        {
                            if (args.Sig.Name == "0") _adminAccess.AddKeytoInput('0');
                            if (args.Sig.Name == "1") _adminAccess.AddKeytoInput('1');
                            if (args.Sig.Name == "2") _adminAccess.AddKeytoInput('2');
                            if (args.Sig.Name == "3") _adminAccess.AddKeytoInput('3');
                            if (args.Sig.Name == "4") _adminAccess.AddKeytoInput('4');
                            if (args.Sig.Name == "5") _adminAccess.AddKeytoInput('5');
                            if (args.Sig.Name == "6") _adminAccess.AddKeytoInput('6');
                            if (args.Sig.Name == "7") _adminAccess.AddKeytoInput('7');
                            if (args.Sig.Name == "8") _adminAccess.AddKeytoInput('8');
                            if (args.Sig.Name == "9") _adminAccess.AddKeytoInput('9');
                        }

                        break;
                    }
                #endregion

                #region Subpage Reference List
                case PanelSmartObjectIDs.SubpageReferenceList:
                    {

                        switch (args.Sig.Type)
                        {
                            case eSigType.Bool:
                                {
                                    #region Volume Control
                                    switch (args.Sig.Number)
                                    {
                                        case 4011:
                                            if (args.Sig.BoolValue) { (_e026Volume[0] as QsysVolume)?.VolumeUpStart(); }
                                            else { (_e026Volume[0] as QsysVolume)?.VolumeUpStop(); }
                                            break;
                                        case 4012:
                                            if (args.Sig.BoolValue) { _e026Volume[0].Mute(); }
                                            break;
                                        case 4013:
                                            if (args.Sig.BoolValue) { _e026Volume[0].VolumeDownStart(); }
                                            else { _e026Volume[0].VolumeDownStop(); }
                                            break;

                                        case 4014:
                                            if (args.Sig.BoolValue) { _e026Volume[1].VolumeUpStart(); }
                                            else { _e026Volume[1].VolumeUpStop(); }
                                            break;
                                        case 4015:
                                            if (args.Sig.BoolValue) { _e026Volume[1].Mute(); }
                                            break;
                                        case 4016:
                                            if (args.Sig.BoolValue) { _e026Volume[1].VolumeDownStart(); }
                                            else { _e026Volume[1].VolumeDownStop(); }
                                            break;

                                        case 4017:
                                            if (args.Sig.BoolValue) { _e026Volume[2].VolumeUpStart(); }
                                            else { _e026Volume[2].VolumeUpStop(); }
                                            break;
                                        case 4018:
                                            if (args.Sig.BoolValue) { _e026Volume[2].Mute(); }
                                            break;
                                        case 4019:
                                            if (args.Sig.BoolValue) { _e026Volume[2].VolumeDownStart(); }
                                            else { _e026Volume[2].VolumeDownStop(); }
                                            break;

                                        case 4020:
                                            if (args.Sig.BoolValue) { _e026Volume[3].VolumeUpStart(); }
                                            else { _e026Volume[3].VolumeUpStop(); }
                                            break;
                                        case 4021:
                                            if (args.Sig.BoolValue) { _e026Volume[3].Mute(); }
                                            break;
                                        case 4022:
                                            if (args.Sig.BoolValue) { _e026Volume[3].VolumeDownStart(); }
                                            else { _e026Volume[3].VolumeDownStop(); }
                                            break;

                                        case 4023:
                                            if (args.Sig.BoolValue) { _e026Volume[4].VolumeUpStart(); }
                                            else { _e026Volume[4].VolumeUpStop(); }
                                            break;
                                        case 4024:
                                            if (args.Sig.BoolValue) { _e026Volume[4].Mute(); }
                                            break;
                                        case 4025:
                                            if (args.Sig.BoolValue) { _e026Volume[4].VolumeDownStart(); }
                                            else { _e026Volume[4].VolumeDownStop(); }
                                            break;

                                        case 4026:
                                            if (args.Sig.BoolValue) { _e026Volume[5].VolumeUpStart(); }
                                            else { _e026Volume[5].VolumeUpStop(); }
                                            break;
                                        case 4027:
                                            if (args.Sig.BoolValue) { _e026Volume[5].Mute(); }
                                            break;
                                        case 4028:
                                            if (args.Sig.BoolValue) { _e026Volume[5].VolumeDownStart(); }
                                            else { _e026Volume[5].VolumeDownStop(); }
                                            break;

                                        case 4029:
                                            if (args.Sig.BoolValue) { _e026Volume[6].VolumeUpStart(); }
                                            else { _e026Volume[6].VolumeUpStop(); }
                                            break;
                                        case 4030:
                                            if (args.Sig.BoolValue) { _e026Volume[6].Mute(); }
                                            break;
                                        case 4031:
                                            if (args.Sig.BoolValue) { _e026Volume[6].VolumeDownStart(); }
                                            else { _e026Volume[6].VolumeDownStop(); }
                                            break;

                                    }
                                    break;
                                    #endregion
                                }
                        }
                        break;
                    }
                    #endregion
            }
        }
        public void E002_PowerOn()
        {
            _selectedReceiver = 5;
            SenkeAn();
            _selectedReceiver = 6;
            SenkeAn();

            _nvxRouter.ChangeRoute((uint)NvxTransmitter.E002ClickShare, (uint)NvXreceiver.E002DisplayLinks);
            _nvxRouter.ChangeRoute((uint)NvxTransmitter.E002ClickShare, (uint)NvXreceiver.E002DisplayRechts);
        }
        public void E002_PowerOff()
        {
            _nvxRouter.Rs232Send((uint)NvXreceiver.E002DisplayLinks, "PowerOn");
            _nvxRouter.Rs232Send((uint)NvXreceiver.E002DisplayRechts, "PowerOn");

            if (!_e002Volume.Getmuted())
            {
                _e002Volume.Mute();
            }
        }
        public void E026_PowerOn()
        {
            _selectedReceiver = 0;
            SenkeAn();
            _selectedReceiver = 1;
            SenkeAn();

            _pageSelection.Activate(0);

            foreach (Volume v in _e026Volume)
            {
                if (v.Getmuted())
                {
                    v.Mute();
                }
            }

            _nvxRouter.ChangeRoute((uint)NvxTransmitter.E026ClickShare, (uint)NvXreceiver.E026Beamer1);
            _nvxRouter.ChangeRoute((uint)NvxTransmitter.E026ClickShare, (uint)NvXreceiver.E026Beamer2);

            //Leinwand1.Motors[1].MotorState = e 
            // TODO  add Motor Control auf baustelle da keine idee was die optionen machen
        }
        public void E026_PowerOff()
        {
            for(int i = 0; i < 7; i++)
            {
                _selectedReceiver =(uint)i ;
                SenkeAus();
            }

            foreach(Volume v in _e026Volume)
            {
                if (!v.Getmuted())
                {
                    v.Mute();
                }
            }
        }
        public void SenkeAn()
        {
            if (_selectedReceiver > 2)
            {
                //Display
                _nvxRouter.Rs232Send(_selectedReceiver, "Einschalten"); //TODO change message and add diffrentiation
            }
            else
            {
                //Beamer
                _nvxRouter.Rs232Send(_selectedReceiver, "Einschalten");
            }
        }
        public void SenkeAus()
        {
            if (_selectedReceiver > 2)
            {
                //Display
                _nvxRouter.Rs232Send(_selectedReceiver, "Ausschalten"); //TODO change message and add diffrentiation
            }
            else
            {
                //Beamer
                _nvxRouter.Rs232Send(_selectedReceiver, "Einschalten");
            }
        }

        public void KopplungChanged()
        {
            if (_kopplung)
            {
                CrestronConsole.PrintLine("deaktiviert");
                _nvxRouter.ChangeRoute(5, 5);
                _nvxRouter.ChangeRoute(5, 6);
                UserInterfaceHelper.SetDigitalJoin(PanelE026, 7, false);
                PanelE002.BooleanInput[22].BoolValue = false;
                _kopplung = false;
            }
            else
            {
                CrestronConsole.PrintLine("aktiviert");
                _nvxRouter.ChangeRoute(_nvxRouter.GetSource(0), 5);
                _nvxRouter.ChangeRoute(_nvxRouter.GetSource(1), 6);
                PanelE002.BooleanInput[22].BoolValue = true;
                UserInterfaceHelper.SetDigitalJoin(PanelE026, 7, true);
                _kopplung=true;
            }
        }

        public enum PanelSmartObjectIDs
        {
            SubpageReferenceList = 1,
            TabButtonVertical = 2,
            Keypad = 3
        }

        public enum NvxTransmitter
        {
            E026ClickShare = 0,
            E026Bodentank1 = 1,
            E026Bodentank2 = 2,
            E026Bodentank3 = 3,
            E026Kamera = 4,
            E002ClickShare = 5,
            E002Bodentank1 = 6,
            E002Bodentank2 = 7
        }

        public enum NvXreceiver
        {
            E026Beamer1 = 0,
            E026Beamer2 = 1,
            E026DisplayLinks = 2,
            E026DisplayRechts = 3,
            E026DisplayVorne = 4,
            E002DisplayLinks = 5,
            E002DisplayRechts = 6,
            E026DisplayMitte = 7
        }

        public override void InitializeSystem()
        {
            try
            {
                InitializePanel();
                Helpers.PrintInfo();
                _nvxRouter = new NvxRouting(this);
                _kopplung = false;
                double volumeIncrement = 0.0045;
                _dsp = new Qsys("192.168.254.238", 1710, "NewUser", "1000");
                _dsp.Logon();

                CrestronConsole.PrintLine("Here");

                _e002Volume = new QsysVolume(PanelE002.UShortInput[2], PanelE002.UShortInput[3], PanelE002.StringInput[3], "Gesamt Lautstärke", 0, volumeIncrement, "gain0", _dsp);

                CrestronConsole.PrintLine("Here");

                _e026Volume = new Volume[9];

                CrestronConsole.PrintLine("Here");
                
                _nvxRouter.Init();

                CrestronConsole.PrintLine("Here");

                InitPageSelection();

                CrestronConsole.PrintLine("Here");
                _senkenWahl = new Interlock();


                /*
                SenkenWahl.AddJoin(Panel_E026.SmartObjects[1].BooleanInput[2], SenkenWahl.addGroup());
                SenkenWahl.AddJoin(Panel_E026.SmartObjects[1].BooleanInput[4], SenkenWahl.addGroup());
                SenkenWahl.AddJoin(Panel_E026.SmartObjects[1].BooleanInput[6], SenkenWahl.addGroup());
                SenkenWahl.AddJoin(Panel_E026.SmartObjects[1].BooleanInput[8], SenkenWahl.addGroup());
                SenkenWahl.AddJoin(Panel_E026.SmartObjects[1].BooleanInput[10], SenkenWahl.addGroup());
                SenkenWahl.AddJoin(Panel_E026.SmartObjects[1].BooleanInput[12], SenkenWahl.addGroup());
                */


                _adminAccess = new Passwort("4690", _pageSelection, 3, PanelE026.StringInput[4]);

                var smartObject = PanelE026.SmartObjects[(int)PanelSmartObjectIDs.SubpageReferenceList];


                _e026Volume[0] = new QsysVolume(smartObject.UShortInput[11], smartObject.UShortInput[12], smartObject.StringInput[11], "Mikrofon 1", 2, volumeIncrement, "gain1", _dsp);
                _e026Volume[1] = new QsysVolume(smartObject.UShortInput[13], smartObject.UShortInput[14], smartObject.StringInput[12], "Mikrofon 2", 2, volumeIncrement, "gain2", _dsp);
                _e026Volume[2] = new QsysVolume(smartObject.UShortInput[15], smartObject.UShortInput[16], smartObject.StringInput[13], "Mikrofon 3", 2, volumeIncrement, "gain3", _dsp);
                _e026Volume[3] = new QsysVolume(smartObject.UShortInput[17], smartObject.UShortInput[18], smartObject.StringInput[14], "Rednerpult", 2, volumeIncrement, "gain4", _dsp);
                _e026Volume[4] = new QsysVolume(smartObject.UShortInput[19], smartObject.UShortInput[20], smartObject.StringInput[15], "Medienton Linker Beamer", 2, volumeIncrement, "gain5", _dsp);
                _e026Volume[5] = new QsysVolume(smartObject.UShortInput[21], smartObject.UShortInput[22], smartObject.StringInput[16], "Medienton Rechter Beamer", 2, volumeIncrement, "gain6", _dsp);
                _e026Volume[6] = new QsysVolume(smartObject.UShortInput[23], smartObject.UShortInput[24], smartObject.StringInput[17], "Mobile Connect", 2, volumeIncrement, "gain7", _dsp);
                _e026Volume[7] = new QsysVolume(smartObject.UShortInput[25], smartObject.UShortInput[26], smartObject.StringInput[18], "Konferenzanlage", 2, volumeIncrement, "gain8", _dsp);
                _e026Volume[8] = new QsysVolume(PanelE026.UShortInput[2], PanelE026.UShortInput[3], PanelE026.StringInput[3], "Gesamt Lautstärke", 0, volumeIncrement, "gain9", _dsp);

                for (int i = 0; i < _e026Volume.Length-1; i++)
                {
                    SmartGraphicsHelper.SetSmartObjectVisible(PanelE026.SmartObjects[1], i+1, true);
                    SmartGraphicsHelper.SetSmartObjectEnable(PanelE026.SmartObjects[1], i+1, true);
                }



            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }

        private void InitPageSelection()
        {
            _pageSelection = new Interlock();
            _pageSelection.AddGroup();
            _pageSelection.AddJoin(PanelE026.BooleanInput[24], 0);
            _pageSelection.AddJoin(PanelE026.BooleanInput[30], 0);
            _pageSelection.AddGroup();
            _pageSelection.AddJoin(PanelE026.BooleanInput[25], 1);
            _pageSelection.AddJoin(PanelE026.BooleanInput[30], 1);
            _pageSelection.AddGroup();
            _pageSelection.AddJoin(PanelE026.BooleanInput[26], 2);
            _pageSelection.AddGroup();
            _pageSelection.AddJoin(PanelE026.BooleanInput[28], 3);
        }



        /// <summary>
        /// Event Handler for Ethernet events: Link Up and Link Down. 
        /// Use these events to close / re-open sockets, etc. 
        /// </summary>
        /// <param name="ethernetEventArgs">This parameter holds the values 
        /// such as whether it's a Link Up or Link Down event. It will also indicate 
        /// wich Ethernet adapter this event belongs to.
        /// </param>
        void _ControllerEthernetEventHandler(EthernetEventArgs ethernetEventArgs)
        {
            switch (ethernetEventArgs.EthernetEventType)
            {//Determine the event type Link Up or Link Down
                case (eEthernetEventType.LinkDown):
                    //Next need to determine which adapter the event is for. 
                    //LAN is the adapter is the port connected to external networks.
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {
                        //
                    }
                    break;
                case (eEthernetEventType.LinkUp):
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {

                    }
                    break;
            }
        }

        /// <summary>
        /// Event Handler for Programmatic events: Stop, Pause, Resume.
        /// Use this event to clean up when a program is stopping, pausing, and resuming.
        /// This event only applies to this SIMPL#Pro program, it doesn't receive events
        /// for other programs stopping
        /// </summary>
        /// <param name="programStatusEventType"></param>
        void _ControllerProgramEventHandler(eProgramStatusEventType programStatusEventType)
        {
            switch (programStatusEventType)
            {
                case (eProgramStatusEventType.Paused):
                    //The program has been paused.  Pause all user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Resumed):
                    //The program has been resumed. Resume all the user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Stopping):
                    //The program has been stopped.
                    //Close all threads. 
                    //Shutdown all Client/Servers in the system.
                    //General cleanup.
                    //Unsubscribe to all System Monitor events
                    break;
            }

        }

        /// <summary>
        /// Event Handler for system events, Disk Inserted/Ejected, and Reboot
        /// Use this event to clean up when someone types in reboot, or when your SD /USB
        /// removable media is ejected / re-inserted.
        /// </summary>
        /// <param name="systemEventType"></param>
        void _ControllerSystemEventHandler(eSystemEventType systemEventType)
        {
            switch (systemEventType)
            {
                case (eSystemEventType.DiskInserted):
                    //Removable media was detected on the system
                    break;
                case (eSystemEventType.DiskRemoved):
                    //Removable media was detached from the system
                    break;
                case (eSystemEventType.Rebooting):
                    //The system is rebooting. 
                    //Very limited time to preform clean up and save any settings to disk.
                    break;
            }

        }

    }
}