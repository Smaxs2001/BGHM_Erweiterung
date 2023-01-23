
using System.Collections.Generic;
using Crestron.SimplSharpPro.DeviceSupport;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace BFE.HelpersX
{
    public class UserInterfaceHelper
    {
        public static void SetDigitalJoin(List<BasicTriListWithSmartObject> devices, uint number, bool value)
        {
            foreach (var d in devices)
            {
                SetDigitalJoin(d, number, value);
            }
        }

        public static void SetDigitalJoin(BasicTriListWithSmartObject currentDevice, uint number, bool value)
        {
            if (currentDevice == null)
            {
                return;
            }
            currentDevice.BooleanInput[number].BoolValue = value;
        }

        public static void SetDigitalJoin(BasicTriListWithSmartObject currentDevice, int number, bool value)
        {
            if (currentDevice == null)
            {
                return;
            }
            currentDevice.BooleanInput[(uint)number].BoolValue = value;
        }

        public static bool GetDigitalJoin(BasicTriListWithSmartObject currentDevice, uint number)
        {
            return currentDevice != null && currentDevice.BooleanOutput[number].BoolValue;
        }

        public static void ToggleDigitalJoin(BasicTriListWithSmartObject currentDevice, uint number)
        {
            if (currentDevice == null)
            {
                return;
            }
            currentDevice.BooleanInput[number].BoolValue = !currentDevice.BooleanInput[number].BoolValue;
        }

        public static void PulseDigitalJoin(List<BasicTriListWithSmartObject> devices, uint number)
        {
            foreach (var d in devices)
            {
                PulseDigitalJoin(d, number);
            }
        }

        public static void PulseDigitalJoin(BasicTriListWithSmartObject currentDevice, uint number)
        {
            currentDevice?.BooleanInput[number].Pulse();
        }

        public static void SetAnalogJoin(List<BasicTriListWithSmartObject> devices, uint number, ushort value)
        {
            foreach (var d in devices)
            {
                SetAnalogJoin(d, number, value);
            }
        }

        public static void SetAnalogJoin(BasicTriListWithSmartObject currentDevice, uint number, ushort value)
        {
            if (currentDevice == null)
            {
                return;
            }
            currentDevice.UShortInput[number].UShortValue = value;
        }

        public static void SetAnalogJoin(List<BasicTriListWithSmartObject> devices, uint number, int value)
        {
            foreach (var d in devices)
            {
                SetAnalogJoin(d, number, value);
            }
        }

        public static void SetAnalogJoin(BasicTriListWithSmartObject currentDevice, uint number, int value)
        {
            if (currentDevice == null)
            {
                return;
            }
            if (value < 0 || value > 65535)
                return;
            currentDevice.UShortInput[number].UShortValue = (ushort)value;
        }

        public static ushort GetAnalogJoin(BasicTriListWithSmartObject currentDevice, uint number)
        {
            return currentDevice?.UShortOutput[number].UShortValue ?? (ushort) 0;
        }

        public static void SetSerialJoin(List<BasicTriListWithSmartObject> devices, uint number, string value)
        {
            foreach (var d in devices)
            {
                SetSerialJoin(d, number, value);
            }
        }

        public static void SetSerialJoin(BasicTriListWithSmartObject currentDevice, uint number, string value)
        {
            if (currentDevice == null)
            {
                return;
            }
            currentDevice.StringInput[number].StringValue = value;
        }

        public static string GetSerialJoin(BasicTriListWithSmartObject currentDevice, uint number)
        {
            return currentDevice == null ? "" : currentDevice.StringOutput[number].StringValue;
        }
    }
}