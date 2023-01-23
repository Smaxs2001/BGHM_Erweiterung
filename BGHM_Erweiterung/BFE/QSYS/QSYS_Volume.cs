using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFE.QSYS
{
    internal class QSYS_Volume : Volume
    {
        private QSYS DSP;
        public string namedControl;
        public QSYS_Volume(UShortInputSig volumeGaugeJoin, UShortInputSig symbolJoin, StringInputSig volumeNameJoin, string volumeName, ushort volumeType, double increment, string namedControl, QSYS DSP) : base(volumeGaugeJoin, symbolJoin, volumeNameJoin, volumeName, volumeType, increment)
        {
            this.namedControl = namedControl;
            this.DSP = DSP;

            this.MinLevel = 0;
            this.MaxLevel = 1;
        }

        override public void UpdateVolume()
        {
            CrestronConsole.PrintLine("UpdateVolume ausgeführt");
            VolumeGaugeJoin.UShortValue=(ushort)(VolumeLevel*65535);
            DSP.SetPosition(namedControl, VolumeLevel);


        }

        override public void mute()
        {
            if (muted)
            {
                DSP.SetValue(namedControl+"mute",false);
                SymbolJoin.UShortValue = (ushort)VolumeType;
            }
            else
            {
                DSP.SetValue(namedControl+"mute", true);
                SymbolJoin.UShortValue = (ushort)(VolumeType+1);
            }

            muted = !muted;
        }

        public void PowerOn()
        {
            //JObject response = DSP.Get(namedControl);

            //VolumeLevel = response;
        }

    }
}
