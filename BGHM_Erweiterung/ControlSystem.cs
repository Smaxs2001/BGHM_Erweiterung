using System;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.Diagnostics;		    	// For System Monitor Access
using Crestron.SimplSharpPro.DeviceSupport;             // For Generic Device Support
using Crestron.SimplSharpPro.DM.Streaming;
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
        private Interlock SenkenWahl;
        private Interlock PageSelection;
        public Ts1070 Panel_E026;
        public Ts1070 Panel_E002;
        private Din2Mc2 Leinwand1;
        private Din2Mc2 Leinwand2;
        private NVXRouting NVXRouter;
        private Passwort AdminAccess;
        private bool Kopplung;
        private Volume E002_Volume;
        private Volume[] E026_Volume;
        private QSYS DSP;


        private uint selected_Receiver = 0;
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
                Panel_E026 = new Ts1070(4, this);
                //Panel_E026.Button

                if (Panel_E026.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    ErrorLog.Error("Failure in myPanel registration = {0}", Panel_E026.RegistrationFailureReason);
                }

                Panel_E002 = new Ts1070(3, this);

                if (Panel_E002.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    ErrorLog.Error("Failure in myPanel registration = {0}", Panel_E002.RegistrationFailureReason);
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
            string SGDFilePath = Path.Combine(Directory.GetApplicationDirectory(), "BGHM_Erweiterung_Groß_V0.1_TS1070.sgd");


            Panel_E026.SigChange +=Panel_E026_SigChange;

            if (File.Exists(SGDFilePath))
            {
                // Load the SGD File into the panel definition
                Panel_E026.LoadSmartObjects(SGDFilePath);

                foreach (KeyValuePair<uint, SmartObject> pair in Panel_E026.SmartObjects)
                {
                    pair.Value.SigChange += MySmartObjectSigChange;
                }
            }
            else
            {
                ErrorLog.Error("SmartGraphics Definition file not found! Set .sgd file to 'Copy Always'!");
            }

            Panel_E002.SigChange +=Panel_E002_SigChange;

        }
        private void Panel_E002_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            switch (args.Sig.Type)
            {
                case eSigType.Bool:
                    {
                        switch (args.Sig.Number)
                        {
                            case 7: NVXRouter.ChangeRoute(5, 5); break;
                            case 8: NVXRouter.ChangeRoute(5, 6); break;
                            case 9: NVXRouter.ChangeRoute(5, 5); NVXRouter.ChangeRoute(5, 6); break;
                            case 10: NVXRouter.ChangeRoute(6, 5); break;
                            case 11: NVXRouter.ChangeRoute(6, 6); break;
                            case 12: NVXRouter.ChangeRoute(6, 5); NVXRouter.ChangeRoute(6, 6); break;
                            case 13: NVXRouter.ChangeRoute(7, 5); break;
                            case 14: NVXRouter.ChangeRoute(7, 6); break;
                            case 15: NVXRouter.ChangeRoute(7, 5); NVXRouter.ChangeRoute(7, 6); break;
                            case 16:
                                if (args.Sig.BoolValue) { E002_Volume.VolumeUpStart(); }
                                else { E002_Volume.VolumeUpStop(); }
                                break;
                            case 17: if (args.Sig.BoolValue) E002_Volume.mute(); break;
                            case 18:
                                if (args.Sig.BoolValue) { E002_Volume.VolumeDownStart(); }
                                else { E002_Volume.VolumeDownStop(); }
                                break;
                            case 20: E002_PowerOff(); break;
                            case 21: E002_PowerOn(); break;

                            default: break;
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
                            if (args.Sig.BoolValue) { E026_Volume[8].VolumeUpStart(); }
                            else { E026_Volume[8].VolumeUpStop(); }
                            break;
                        }
                        if (args.Sig.Number == 11)
                        {
                            if (args.Sig.BoolValue) { E026_Volume[8].mute(); }
                            break;
                        }
                        if (args.Sig.Number == 10)
                        {
                            if (args.Sig.BoolValue) { E026_Volume[8].VolumeDownStart(); }
                            else { E026_Volume[8].VolumeDownStop(); }
                            break;
                        }

                        if (args.Sig.BoolValue)
                        {
                            if (args.Sig.Number == 7) KopplungChanged();

                            if (args.Sig.Number == 8) PageSelection.activate(0);

                            if (args.Sig.Number == 10) E026_PowerOn();

                            if (args.Sig.Number > 15 && args.Sig.Number <21)
                            {
                                NVXRouter.ChangeRoute(args.Sig.Number-16, selected_Receiver);
                                if (Kopplung)
                                {
                                    if (selected_Receiver == 0)
                                    {
                                        NVXRouter.ChangeRoute(args.Sig.Number-16, 5);
                                    }
                                    if (selected_Receiver == 1)
                                    {
                                        NVXRouter.ChangeRoute(args.Sig.Number-16, 6);
                                    }
                                }
                            }
                            if (args.Sig.Number == 21)
                            {
                                NVXRouter.NvxBlack(selected_Receiver);
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
                                PageSelection.activate(0);
                            }
                            if (args.Sig.Number == 25)
                            {
                                CrestronConsole.PrintLine("Hallo");
                                PageSelection.activate(1);
                            }
                            if (args.Sig.Number == 26)
                            {
                                CrestronConsole.PrintLine("Hallo");
                                PageSelection.activate(2);
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
                                    SenkenWahl.activate(0);
                                    selected_Receiver = 0;
                                    Panel_E026.StringInput[2].StringValue = ((NvxTransmitter)NVXRouter.getSource(selected_Receiver)).ToString();
                                    Panel_E026.StringInput[5].StringValue = "Beamer 1 ausgewählt";
                                    break;
                                }
                            case ("Tab Button 2 Press"):
                                {
                                    SenkenWahl.activate(1);
                                    selected_Receiver = 1;
                                    Panel_E026.StringInput[2].StringValue = ((NvxTransmitter)NVXRouter.getSource(selected_Receiver)).ToString();
                                    Panel_E026.StringInput[5].StringValue = "Beamer 2 ausgewählt";
                                    break;
                                }
                            case ("Tab Button 3 Press"):
                                {
                                    SenkenWahl.activate(2);
                                    selected_Receiver = 2;
                                    Panel_E026.StringInput[2].StringValue = ((NvxTransmitter)NVXRouter.getSource(selected_Receiver)).ToString();
                                    Panel_E026.StringInput[5].StringValue = "Display Links ausgewählt";
                                    break;
                                }
                            case ("Tab Button 4 Press"):
                                {
                                    SenkenWahl.activate(3);
                                    selected_Receiver = 7;
                                    Panel_E026.StringInput[2].StringValue = ((NvxTransmitter)NVXRouter.getSource(selected_Receiver)).ToString();
                                    Panel_E026.StringInput[5].StringValue = "Display Mitte ausgewählt";
                                    break;
                                }
                            case ("Tab Button 5 Press"):
                                {
                                    SenkenWahl.activate(4);
                                    selected_Receiver = 3;
                                    Panel_E026.StringInput[2].StringValue = ((NvxTransmitter)NVXRouter.getSource(selected_Receiver)).ToString();
                                    Panel_E026.StringInput[5].StringValue = "Display Rechts ausgewählt";
                                    break;
                                }
                            case ("Tab Button 6 Press"):
                                {
                                    SenkenWahl.activate(5);
                                    selected_Receiver = 4;
                                    Panel_E026.StringInput[2].StringValue = ((NvxTransmitter)NVXRouter.getSource(selected_Receiver)).ToString();
                                    Panel_E026.StringInput[5].StringValue = "Display Vorne ausgewählt";
                                    break;
                                }

                            default: break;
                        }
                        break;
                    }
                #endregion

                #region Keypad Smartobject
                case PanelSmartObjectIDs.Keypad:
                    {
                        if (args.Sig.Name == "Misc_1")
                        {
                            AdminAccess.exit();
                        }
                        if (args.Sig.Name == "Misc_2")
                        {
                            AdminAccess.checkPassword();
                        }
                        if (args.Sig.BoolValue == true)
                        {
                            if (args.Sig.Name == "0") AdminAccess.addKeytoInput('0');
                            if (args.Sig.Name == "1") AdminAccess.addKeytoInput('1');
                            if (args.Sig.Name == "2") AdminAccess.addKeytoInput('2');
                            if (args.Sig.Name == "3") AdminAccess.addKeytoInput('3');
                            if (args.Sig.Name == "4") AdminAccess.addKeytoInput('4');
                            if (args.Sig.Name == "5") AdminAccess.addKeytoInput('5');
                            if (args.Sig.Name == "6") AdminAccess.addKeytoInput('6');
                            if (args.Sig.Name == "7") AdminAccess.addKeytoInput('7');
                            if (args.Sig.Name == "8") AdminAccess.addKeytoInput('8');
                            if (args.Sig.Name == "9") AdminAccess.addKeytoInput('9');
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
                                            if (args.Sig.BoolValue) { (E026_Volume[0] as QSYS_Volume).VolumeUpStart(); }
                                            else { (E026_Volume[0] as QSYS_Volume).VolumeUpStop(); }
                                            break;
                                        case 4012:
                                            if (args.Sig.BoolValue) { E026_Volume[0].mute(); }
                                            break;
                                        case 4013:
                                            if (args.Sig.BoolValue) { E026_Volume[0].VolumeDownStart(); }
                                            else { E026_Volume[0].VolumeDownStop(); }
                                            break;

                                        case 4014:
                                            if (args.Sig.BoolValue) { E026_Volume[1].VolumeUpStart(); }
                                            else { E026_Volume[1].VolumeUpStop(); }
                                            break;
                                        case 4015:
                                            if (args.Sig.BoolValue) { E026_Volume[1].mute(); }
                                            break;
                                        case 4016:
                                            if (args.Sig.BoolValue) { E026_Volume[1].VolumeDownStart(); }
                                            else { E026_Volume[1].VolumeDownStop(); }
                                            break;

                                        case 4017:
                                            if (args.Sig.BoolValue) { E026_Volume[2].VolumeUpStart(); }
                                            else { E026_Volume[2].VolumeUpStop(); }
                                            break;
                                        case 4018:
                                            if (args.Sig.BoolValue) { E026_Volume[2].mute(); }
                                            break;
                                        case 4019:
                                            if (args.Sig.BoolValue) { E026_Volume[2].VolumeDownStart(); }
                                            else { E026_Volume[2].VolumeDownStop(); }
                                            break;

                                        case 4020:
                                            if (args.Sig.BoolValue) { E026_Volume[3].VolumeUpStart(); }
                                            else { E026_Volume[3].VolumeUpStop(); }
                                            break;
                                        case 4021:
                                            if (args.Sig.BoolValue) { E026_Volume[3].mute(); }
                                            break;
                                        case 4022:
                                            if (args.Sig.BoolValue) { E026_Volume[3].VolumeDownStart(); }
                                            else { E026_Volume[3].VolumeDownStop(); }
                                            break;

                                        case 4023:
                                            if (args.Sig.BoolValue) { E026_Volume[4].VolumeUpStart(); }
                                            else { E026_Volume[4].VolumeUpStop(); }
                                            break;
                                        case 4024:
                                            if (args.Sig.BoolValue) { E026_Volume[4].mute(); }
                                            break;
                                        case 4025:
                                            if (args.Sig.BoolValue) { E026_Volume[4].VolumeDownStart(); }
                                            else { E026_Volume[4].VolumeDownStop(); }
                                            break;

                                        case 4026:
                                            if (args.Sig.BoolValue) { E026_Volume[5].VolumeUpStart(); }
                                            else { E026_Volume[5].VolumeUpStop(); }
                                            break;
                                        case 4027:
                                            if (args.Sig.BoolValue) { E026_Volume[5].mute(); }
                                            break;
                                        case 4028:
                                            if (args.Sig.BoolValue) { E026_Volume[5].VolumeDownStart(); }
                                            else { E026_Volume[5].VolumeDownStop(); }
                                            break;

                                        case 4029:
                                            if (args.Sig.BoolValue) { E026_Volume[6].VolumeUpStart(); }
                                            else { E026_Volume[6].VolumeUpStop(); }
                                            break;
                                        case 4030:
                                            if (args.Sig.BoolValue) { E026_Volume[6].mute(); }
                                            break;
                                        case 4031:
                                            if (args.Sig.BoolValue) { E026_Volume[6].VolumeDownStart(); }
                                            else { E026_Volume[6].VolumeDownStop(); }
                                            break;

                                        default: break;
                                    }
                                    break;
                                    #endregion
                                }
                            default: break;
                        }
                        break;
                    }
                    #endregion
            }
        }
        public void E002_PowerOn()
        {
            selected_Receiver = 5;
            SenkeAn();
            selected_Receiver = 6;
            SenkeAn();

            NVXRouter.ChangeRoute((uint)NvxTransmitter.E002_ClickShare, (uint)NVXreceiver.E002_Display_Links);
            NVXRouter.ChangeRoute((uint)NvxTransmitter.E002_ClickShare, (uint)NVXreceiver.E002_Display_rechts);
        }
        public void E002_PowerOff()
        {
            NVXRouter.RS232Send((uint)NVXreceiver.E002_Display_Links, "PowerOn");
            NVXRouter.RS232Send((uint)NVXreceiver.E002_Display_rechts, "PowerOn");

            if (!E002_Volume.getmuted())
            {
                E002_Volume.mute();
            }
        }
        public void E026_PowerOn()
        {
            selected_Receiver = 0;
            SenkeAn();
            selected_Receiver = 1;
            SenkeAn();

            PageSelection.activate(0);

            foreach (Volume V in E026_Volume)
            {
                if (V.getmuted())
                {
                    V.mute();
                }
            }

            NVXRouter.ChangeRoute((uint)NvxTransmitter.E026_ClickShare, (uint)NVXreceiver.E026_beamer1);
            NVXRouter.ChangeRoute((uint)NvxTransmitter.E026_ClickShare, (uint)NVXreceiver.E026_beamer2);

            //Leinwand1.Motors[1].MotorState = e 
            // TODO  add Motor Control auf baustelle da keine idee was die optionen machen
        }
        public void E026_PowerOff()
        {
            for(int i = 0; i < 7; i++)
            {
                selected_Receiver =(uint)i ;
                SenkeAus();
            }

            foreach(Volume V in E026_Volume)
            {
                if (!V.getmuted())
                {
                    V.mute();
                }
            }
        }
        public void SenkeAn()
        {
            if (selected_Receiver > 2)
            {
                //Display
                NVXRouter.RS232Send(selected_Receiver, "Einschalten"); //TODO change message and add diffrentiation
            }
            else
            {
                //Beamer
                NVXRouter.RS232Send(selected_Receiver, "Einschalten");
            }
        }
        public void SenkeAus()
        {
            if (selected_Receiver > 2)
            {
                //Display
                NVXRouter.RS232Send(selected_Receiver, "Ausschalten"); //TODO change message and add diffrentiation
            }
            else
            {
                //Beamer
                NVXRouter.RS232Send(selected_Receiver, "Einschalten");
            }
        }

        public void KopplungChanged()
        {
            if (Kopplung)
            {
                CrestronConsole.PrintLine("deaktiviert");
                NVXRouter.ChangeRoute(5, 5);
                NVXRouter.ChangeRoute(5, 6);
                UserInterfaceHelper.SetDigitalJoin(Panel_E026, 7, false);
                Panel_E002.BooleanInput[22].BoolValue = false;
                Kopplung = false;
            }
            else
            {
                CrestronConsole.PrintLine("aktiviert");
                NVXRouter.ChangeRoute(NVXRouter.getSource(0), 5);
                NVXRouter.ChangeRoute(NVXRouter.getSource(1), 6);
                Panel_E002.BooleanInput[22].BoolValue = true;
                UserInterfaceHelper.SetDigitalJoin(Panel_E026, 7, true);
                Kopplung=true;
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
            E026_ClickShare = 0,
            E026_Bodentank1 = 1,
            E026_Bodentank2 = 2,
            E026_Bodentank3 = 3,
            E026_Kamera = 4,
            E002_ClickShare = 5,
            E002_Bodentank1 = 6,
            E002_Bodentank2 = 7
        }

        public enum NVXreceiver
        {
            E026_beamer1 = 0,
            E026_beamer2 = 1,
            E026_Display_Links = 2,
            E026_Display_rechts = 3,
            E026_Display_vorne = 4,
            E002_Display_Links = 5,
            E002_Display_rechts = 6,
            E026_Display_Mitte = 7
        }

        public override void InitializeSystem()
        {
            try
            {
                InitializePanel();

                Helpers.PrintInfo();
                NVXRouter = new NVXRouting(this);
                CrestronConsole.PrintLine("Here");
                Kopplung = false;
                double VolumeIncrement = 0.0045;
                CrestronConsole.PrintLine("Here");
                DSP = new QSYS("192.168.254.238", 1710, "NewUser", "1000");
                CrestronConsole.PrintLine("Here");
                DSP.LOGON();

                CrestronConsole.PrintLine("Here");

                E002_Volume = new QSYS_Volume(Panel_E002.UShortInput[2], Panel_E002.UShortInput[3], Panel_E002.StringInput[3], "Gesamt Lautstärke", 0, VolumeIncrement, "gain0", DSP);

                CrestronConsole.PrintLine("Here");

                E026_Volume = new Volume[9];

                CrestronConsole.PrintLine("Here");
                
                NVXRouter.Init();

                CrestronConsole.PrintLine("Here");

                InitPageSelection();

                CrestronConsole.PrintLine("Here");
                SenkenWahl = new Interlock();


                /*
                SenkenWahl.AddJoin(Panel_E026.SmartObjects[1].BooleanInput[2], SenkenWahl.addGroup());
                SenkenWahl.AddJoin(Panel_E026.SmartObjects[1].BooleanInput[4], SenkenWahl.addGroup());
                SenkenWahl.AddJoin(Panel_E026.SmartObjects[1].BooleanInput[6], SenkenWahl.addGroup());
                SenkenWahl.AddJoin(Panel_E026.SmartObjects[1].BooleanInput[8], SenkenWahl.addGroup());
                SenkenWahl.AddJoin(Panel_E026.SmartObjects[1].BooleanInput[10], SenkenWahl.addGroup());
                SenkenWahl.AddJoin(Panel_E026.SmartObjects[1].BooleanInput[12], SenkenWahl.addGroup());
                */


                AdminAccess = new Passwort("4690", PageSelection, 3, Panel_E026.StringInput[4]);

                var SmartObject = Panel_E026.SmartObjects[(int)PanelSmartObjectIDs.SubpageReferenceList];


                E026_Volume[0] = new QSYS_Volume(SmartObject.UShortInput[11], SmartObject.UShortInput[12], SmartObject.StringInput[11], "Mikrofon 1", 2, VolumeIncrement, "gain1", DSP);
                E026_Volume[1] = new QSYS_Volume(SmartObject.UShortInput[13], SmartObject.UShortInput[14], SmartObject.StringInput[12], "Mikrofon 2", 2, VolumeIncrement, "gain2", DSP);
                E026_Volume[2] = new QSYS_Volume(SmartObject.UShortInput[15], SmartObject.UShortInput[16], SmartObject.StringInput[13], "Mikrofon 3", 2, VolumeIncrement, "gain3", DSP);
                E026_Volume[3] = new QSYS_Volume(SmartObject.UShortInput[17], SmartObject.UShortInput[18], SmartObject.StringInput[14], "Rednerpult", 2, VolumeIncrement, "gain4", DSP);
                E026_Volume[4] = new QSYS_Volume(SmartObject.UShortInput[19], SmartObject.UShortInput[20], SmartObject.StringInput[15], "Medienton Linker Beamer", 2, VolumeIncrement, "gain5", DSP);
                E026_Volume[5] = new QSYS_Volume(SmartObject.UShortInput[21], SmartObject.UShortInput[22], SmartObject.StringInput[16], "Medienton Rechter Beamer", 2, VolumeIncrement, "gain6", DSP);
                E026_Volume[6] = new QSYS_Volume(SmartObject.UShortInput[23], SmartObject.UShortInput[24], SmartObject.StringInput[17], "Mobile Connect", 2, VolumeIncrement, "gain7", DSP);
                E026_Volume[7] = new QSYS_Volume(SmartObject.UShortInput[25], SmartObject.UShortInput[26], SmartObject.StringInput[18], "Konferenzanlage", 2, VolumeIncrement, "gain8", DSP);
                E026_Volume[8] = new QSYS_Volume(Panel_E026.UShortInput[2], Panel_E026.UShortInput[3], Panel_E026.StringInput[3], "Gesamt Lautstärke", 0, VolumeIncrement, "gain9", DSP);

                for (int i = 0; i < E026_Volume.Length-1; i++)
                {
                    SmartGraphicsHelper.SetSmartObjectVisible(Panel_E026.SmartObjects[1], i+1, true);
                    SmartGraphicsHelper.SetSmartObjectEnable(Panel_E026.SmartObjects[1], i+1, true);
                }



            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }

        private void InitPageSelection()
        {
            PageSelection = new Interlock();
            PageSelection.addGroup();
            PageSelection.AddJoin(Panel_E026.BooleanInput[24], 0);
            PageSelection.AddJoin(Panel_E026.BooleanInput[30], 0);
            PageSelection.addGroup();
            PageSelection.AddJoin(Panel_E026.BooleanInput[25], 1);
            PageSelection.AddJoin(Panel_E026.BooleanInput[30], 1);
            PageSelection.addGroup();
            PageSelection.AddJoin(Panel_E026.BooleanInput[26], 2);
            PageSelection.addGroup();
            PageSelection.AddJoin(Panel_E026.BooleanInput[28], 3);
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