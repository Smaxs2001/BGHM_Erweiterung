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
    internal class QsysVolume : Volume
    {
        private Qsys _dsp;
        public string NamedControl;
        public QsysVolume(UShortInputSig volumeGaugeJoin, UShortInputSig symbolJoin, StringInputSig volumeNameJoin, string volumeName, ushort volumeType, double increment, string namedControl, Qsys dsp) : base(volumeGaugeJoin, symbolJoin, volumeNameJoin, volumeName, volumeType, increment)
        {
            this.NamedControl = namedControl;
            this._dsp = dsp;

            this.MinLevel = 0;
            this.MaxLevel = 1;
        }

        override public void UpdateVolume()
        {
            CrestronConsole.PrintLine("UpdateVolume ausgeführt");
            VolumeGaugeJoin.UShortValue=(ushort)(VolumeLevel*65535);
            _dsp.SetPosition(NamedControl, VolumeLevel);


        }

        override public void Mute()
        {
            if (Muted)
            {
                _dsp.SetValue(NamedControl+"mute",false);
                SymbolJoin.UShortValue = (ushort)VolumeType;
            }
            else
            {
                _dsp.SetValue(NamedControl+"mute", true);
                SymbolJoin.UShortValue = (ushort)(VolumeType+1);
            }

            Muted = !Muted;
        }

        public void PowerOn()
        {
            //JObject response = DSP.Get(namedControl);

            //VolumeLevel = response;
        }

    }
}
