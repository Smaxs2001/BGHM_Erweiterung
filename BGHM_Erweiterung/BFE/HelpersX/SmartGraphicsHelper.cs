using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace BFE.HelpersX
{
    public class SmartGraphicsHelper
    {
        // Crestrons implementation of Smart joins is terrible, hopefully this library helps.
        // use inputs (e.g. BooleanInput as opposed to BooleanOutput) for setting values.

        #region digitals

        // Standard lists BooleanInput["Item 1 Pressed" ]
        // Dynamic  lists BooleanInput["Item 1 Selected"]
        public static void SetSmartObjectDigitalJoin(SmartObject so, string name, bool state)
        {
            if (so == null || name == null)
                return;
            try
            {
                so.BooleanInput[name].BoolValue = state;
            }
            catch (Exception)
            {
                //ErrorLog.Notice("SetSmartObjectDigitalJoin exception: {0}", e.Message);
            }
        }

        public static void SetSmartObjectSelected(SmartObject so, int index, bool state)
        {
            if (so == null)
                return;
            var name = "Item " + index + " Selected";
            if (so.StringInput.Contains(name))
                SetSmartObjectDigitalJoin(so, name, state);
            else
            {
                name = "Item " + index + " Pressed";
                if (so.StringInput.Contains(name))
                    SetSmartObjectDigitalJoin(so, name, state);
                else
                    so.BooleanInput[(ushort)index].BoolValue = state;
            }
        }
        public static void SetSmartObjectSelected(BasicTriListWithSmartObject device, uint smartObjectId, int index, bool state)
        {
            if (device == null)
                return;
            SetSmartObjectSelected(device.SmartObjects[smartObjectId], index, state);
        }
        public static void ToggleSmartObjectSelected(SmartObject so, int index)
        {
            if (so == null)
                return;
            try
            {
                var name = "Item " + index + " Selected";
                if (so.StringInput.Contains(name))
                    so.BooleanInput[name].BoolValue = !so.BooleanInput[name].BoolValue;
                else
                {
                    name = "Item " + index + " Pressed";
                    if (so.StringInput.Contains(name))
                        so.BooleanInput[name].BoolValue = !so.BooleanInput[name].BoolValue;
                    else
                        so.BooleanInput[(ushort)index].BoolValue = !so.BooleanInput[(ushort)index].BoolValue;
                }
            }
            catch (Exception e)
            {
                ErrorLog.Notice("ToggleSmartObjectSelected exception: {0}", e.Message);
            }
        }
        public static void ToggleSmartObjectSelected(BasicTriListWithSmartObject device, uint smartObjectId, int index)
        {
            if (device == null)
                return;
            ToggleSmartObjectSelected(device.SmartObjects[smartObjectId], index);
        }

        public static void SetSmartObjectVisible(SmartObject so, int index, bool state)
        {
            if (so == null)
                return;
            var name = "Item " + index + " Visible";
            //ErrorLog.Notice("SO visible name: {0}", name);
            SetSmartObjectDigitalJoin(so, name, state);
        }
        public static void SetSmartObjectVisible(BasicTriListWithSmartObject device, uint smartObjectId, int index, bool state)
        {
            if (device == null)
                return;
            SetSmartObjectVisible(device.SmartObjects[smartObjectId], index, state);
        }
        public static void ToggleSmartObjectVisible(SmartObject so, int index)
        {
            if (so == null)
                return;
            try
            {
                var name = "Item " + index + " Visible";
                so.BooleanInput[name].BoolValue = !so.BooleanInput[name].BoolValue;
            }
            catch (Exception e)
            {
                ErrorLog.Notice("ToggleSmartObjectVisible exception: {0}", e.Message);
            }
        }
        public static void ToggleSmartObjectVisible(BasicTriListWithSmartObject device, uint smartObjectId, int index)
        {
            if (device == null)
                return;
            ToggleSmartObjectVisible(device.SmartObjects[smartObjectId], index);
        }

        public static void SetSmartObjectEnable(SmartObject so, int index, bool state)
        {
            if (so == null)
                return;
            var name = "Item " + index + " Enable";
            SetSmartObjectDigitalJoin(so, name, state);
        }
        public static void SetSmartObjectEnabled(SmartObject so, int index, bool state)
        {
            if (so == null)
                return;
            var name = "Item " + index + " Enabled";
            SetSmartObjectDigitalJoin(so, name, state);
        }
        public static void SetSmartObjectEnabled(BasicTriListWithSmartObject device, uint smartObjectId, int index, bool state)
        {
            if (device == null)
                return;
            SetSmartObjectEnabled(device.SmartObjects[smartObjectId], index, state);
        }
        public static void ToggleSmartObjectEnabled(SmartObject so, int index)
        {
            if (so == null)
                return;
            try
            {
                var name = "Item " + index + " Enabled";
                so.BooleanInput[name].BoolValue = !so.BooleanInput[name].BoolValue;
            }
            catch (Exception e)
            {
                ErrorLog.Notice("ToggleSmartObjectEnable exception: {0}", e.Message);
            }
        }
        public static void ToggleSmartObjectEnabled(BasicTriListWithSmartObject device, uint smartObjectId, int index)
        {
            if (device == null)
                return;
            ToggleSmartObjectEnabled(device.SmartObjects[smartObjectId], index);
        }

        public static bool GetSmartObjectDigitalJoin(SmartObject so, string name)
        {
            try
            {
                return so.BooleanInput[name].BoolValue;
            }
            catch (Exception e)
            {
                ErrorLog.Notice("GetSmartObjectDigitalJoin exception: {0}", e.Message);
            }
            return false;
        }
        public static bool GetSmartObjectDigitalJoin(BasicTriListWithSmartObject device, uint smartObjectId, string name)
        {
            return GetSmartObjectDigitalJoin(device.SmartObjects[smartObjectId], name);
        }
        public static bool GetSmartObjectDigitalJoin(SmartObject so, int index)
        {
            var name = "Item " + index + " Selected";
            if (so.StringInput.Contains(name))
                return so.BooleanInput[name].BoolValue;
            else
            {
                name = "Item " + index + " Pressed";
                if (so.StringInput.Contains(name))
                    return so.BooleanInput[name].BoolValue;
                else
                    return so.BooleanInput[(ushort)index].BoolValue;
            }

        }
        public static bool GetSmartObjectDigitalJoin(BasicTriListWithSmartObject device, uint smartObjectId, int index)
        {
            return GetSmartObjectDigitalJoin(device.SmartObjects[smartObjectId], index);
        }

        public static void SetSmartObjectDigitalListJoin(SmartObject so, int index, bool state)
        {
            if (so == null)
                return;
            var name = "fb" + index;
            SetSmartObjectDigitalJoin(so, name, state);
        }

        // SmartObject join number doesn't necessarily start at 1 so use the "selected" functions unless you are sure you have the right join
        public static void SetSmartObjectDigitalJoin(SmartObject so, int index, bool state)
        {
            try
            {
                so.BooleanInput[(ushort)index].BoolValue = state;
            }
            catch (Exception)
            {
                //ErrorLog.Notice("SetSmartObjectDigitalJoin exception: {0}", e.Message);
            }
        }
        public static void SetSmartObjectDigitalJoin(BasicTriListWithSmartObject device, uint smartObjectId, int index, bool state)
        {
            SetSmartObjectDigitalJoin(device.SmartObjects[smartObjectId], index, state);
        }
        public static void ToggleSmartObjectDigitalJoin(SmartObject so, int index)
        {
            try
            {
                so.BooleanInput[(ushort)index].BoolValue = !so.BooleanInput[(ushort)index].BoolValue;
            }
            catch (Exception e)
            {
                ErrorLog.Notice("ToggleSmartObjectDigitalJoin exception: {0}", e.Message);
            }
        }
        public static void ToggleSmartObjectDigitalJoin(BasicTriListWithSmartObject device, uint smartObjectId, int index)
        {
            ToggleSmartObjectDigitalJoin(device.SmartObjects[smartObjectId], index);
        }

        #endregion

        #region analogs

        // most analogs use format "Set Item 1 Text"
        public static void SetSmartObjectValue(SmartObject so, uint id, ushort state)
        {
            try
            {
                so.UShortInput[id].UShortValue = state;
            }
            catch (Exception e)
            {
                ErrorLog.Notice("SetSmartObjectValue exception: {0}", e.Message);
            }
        }

        public static void SetSmartObjectValue(SmartObject so, string name, ushort state)
        {
            try
            {
                so.UShortInput[name].UShortValue = state;
            }
            catch (Exception e)
            {
                ErrorLog.Notice("SetSmartObjectValue exception: {0}", e.Message);
            }
        }
        public static void SetSmartObjectValue(BasicTriListWithSmartObject device, uint smartObjectId, string name, ushort state)
        {
            SetSmartObjectValue(device.SmartObjects[smartObjectId], name, state);
        }
        public static void SetSmartObjectValue(SmartObject so, int index, ushort state)
        {
            try
            {
                so.UShortInput[(ushort)index].UShortValue = state;
            }
            catch (Exception e)
            {
                ErrorLog.Notice("SetSmartObjectDigitalJoin exception: {0}", e.Message);
            }
        }
        public static void SetSmartObjectValue(BasicTriListWithSmartObject device, uint smartObjectId, int index, ushort state)
        {
            SetSmartObjectValue(device.SmartObjects[smartObjectId], index, state);
        }
        // Icons "Set Item 1 Analog"
        public static void SetSmartObjectIconAnalog(SmartObject so, int index, ushort state)
        {
            var name = "Set Item " + index + " Icon Analog";
            SetSmartObjectValue(so, name, state);
        }
        public static void SetSmartObjectNumberItems(SmartObject so, int nr)
        {
            SetSmartObjectNumberItems(so, (ushort)nr);
        }
        public static void SetSmartObjectNumberItems(SmartObject so, ushort nr)
        {
            var name = "Set Number of Items";
            SetSmartObjectValue(so, name, nr);
        }
        public static void SetSmartObjectSelectItem(SmartObject so, ushort item)
        {
            var name = "Select Item";
            SetSmartObjectValue(so, name, item);
        }

        public static void SetSmartObjectAnalogJoin(SmartObject so, int index, ushort state)
        {
            var name = "an_fb" + index;
            SetSmartObjectValue(so, name, state);
        }

        public static ushort GetSmartObjectAnalogJoin(SmartObject so, string name)
        {
            try
            {
                return so.UShortInput[name].UShortValue;
            }
            catch (Exception e)
            {
                ErrorLog.Notice("GetSmartObjectAnalogJoin exception: {0}", e.Message);
            }
            return 0;
        }
        public static ushort GetSmartObjectAnalogJoin(BasicTriListWithSmartObject device, uint smartObjectId, string name)
        {
            return GetSmartObjectAnalogJoin(device.SmartObjects[smartObjectId], name);
        }
        public static ushort GetSmartObjectAnalogJoin(SmartObject so, int index)
        {
            var name = "Item Selected";
            if (so.StringInput.Contains(name))
                return so.UShortInput[name].UShortValue;
            else
            {
                name = "Item " + index + " Pressed";
                if (so.StringInput.Contains(name))
                    return so.UShortInput[name].UShortValue;
                else
                    return so.UShortInput[(ushort)index].UShortValue;
            }

        }
        public static ushort GetSmartObjectAnalogJoin(BasicTriListWithSmartObject device, uint smartObjectId, int index)
        {
            return GetSmartObjectAnalogJoin(device.SmartObjects[smartObjectId], index);
        }

        #endregion

        #region serials

        public static void SetSmartObjectText(SmartObject so, string name, string state)
        {
            try
            {
                if (so.StringInput.Contains(name))
                    so.StringInput[name].StringValue = state;
                else
                    ErrorLog.Notice("SmartObject does not contain {0} to set {1}", name, state);
            }
            catch (Exception e)
            {
                ErrorLog.Notice("SetSmartObjectText exception: {0}", e.Message);
            }
        }
        public static void SetSmartObjectText(SmartObject so, int index, string state)
        {
            // Standard lists use serial format "Set Item 1 Text"
            // Dynamic lists use serial format "Item 1 Text"
            var name = "Item " + index + " Text";
            if (!so.StringInput.Contains(name))
                name = "Set " + name;
            SetSmartObjectText(so, name, state);
        }
        public static void SetSmartObjectText(BasicTriListWithSmartObject device, uint smartObjectId, int index, string state)
        {
            SetSmartObjectText(device.SmartObjects[smartObjectId], index, state);
        }
        // some serials use format "text-i1"
        public static void SetSmartObjectInputText(SmartObject so, int index, string state)
        {
            var name = "text-o" + index;
            SetSmartObjectText(so, name, state);
        }

        public static void SetSmartObjectItemText(SmartObject so, int index, string text)
        {
            var name = "Set Item " + index + " Text";
            SetSmartObjectText(so, name, text);
        }

        // icon text matches 
        public static void SetSmartObjectIconSerial(SmartObject so, int index, string iconName)
        {
            var name = "Set Item " + index + " Icon Serial";
            SetSmartObjectText(so, name, iconName);
        }

        #endregion

        public static void PrintSmartObjectData(SmartObject so)
        {
            foreach (var sig in so.BooleanInput)
                PrintSig(sig);
            foreach (var sig in so.BooleanOutput)
                PrintSig(sig);
            foreach (var sig in so.UShortInput)
                PrintSig(sig);
            foreach (var sig in so.UShortOutput)
                PrintSig(sig);
            foreach (var sig in so.StringInput)
                PrintSig(sig);
            foreach (var sig in so.StringOutput)
                PrintSig(sig);
        }

        static void PrintSig(Sig sig)
        {
            ErrorLog.Notice("{0}[Name: {1} | Join: {2}]", sig.GetType().Name, sig.Name, sig.Number);
        }
    }
    
}