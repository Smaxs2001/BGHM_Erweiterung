using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Independentsoft.Exchange;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BFE.HelpersX;

namespace BFE
{



    internal class Volume
    {
        protected UShortInputSig VolumeGaugeJoin;
        protected UShortInputSig SymbolJoin;
        protected StringInputSig VolumeNameJoin;
        protected string VolumeName;
        protected ushort VolumeType;
        protected double Increment;

        private System.Timers.Timer _volumeUpTimer;
        private System.Timers.Timer _volumeDownTimer;

        protected double MaxLevel = 65535;
        protected double MinLevel = 0;
        protected double VolumeLevel;
        private double _lastLevel;
        protected bool Muted = false;




        public Volume(UShortInputSig volumeGaugeJoin, UShortInputSig symbolJoin, StringInputSig volumeNameJoin, string volumeName, ushort volumeType,double increment)
        {
            VolumeGaugeJoin=volumeGaugeJoin;
            SymbolJoin=symbolJoin;
            VolumeNameJoin=volumeNameJoin;
            VolumeName=volumeName;
            VolumeType=volumeType;
            Increment=increment;

            VolumeLevel = 0;

            VolumeNameJoin.StringValue = volumeName;
            SymbolJoin.UShortValue = VolumeType;

            _volumeUpTimer = new System.Timers.Timer();
            _volumeUpTimer.Interval = 60;
            _volumeUpTimer.AutoReset = true;
            _volumeUpTimer.Elapsed += VolumeUp;

            _volumeDownTimer = new System.Timers.Timer();
            _volumeDownTimer.Interval = 60;
            _volumeDownTimer.AutoReset = true;
            _volumeDownTimer.Elapsed += VolumeDown;
        }

        public bool Getmuted()
        {
            return Muted;
        }
        virtual public void UpdateVolume()
        {
            VolumeGaugeJoin.UShortValue=(ushort)(VolumeLevel);
        }

        public void VolumeUpStart()
        {
            if (Muted) return;
            _volumeUpTimer.Start();
        }

        public void VolumeUpStop()
        {
            _volumeUpTimer.Stop();
        }
        private void VolumeUp(object sender,EventArgs args)
        {
            
            VolumeLevel = (VolumeLevel + Increment);
            if(VolumeLevel > MaxLevel-Increment)
            {
                _volumeUpTimer.Stop();
                VolumeLevel = MaxLevel;
            }

            UpdateVolume();

        }

        public void VolumeDownStart()
        {
            if (Muted) return;
            _volumeDownTimer.Start();
        }

        public void VolumeDownStop()
        {
            _volumeDownTimer.Stop();
        }
        private void VolumeDown(object sender, EventArgs args)
        {
            VolumeLevel = (VolumeLevel - Increment);
            if (VolumeLevel < MinLevel+Increment)
            {
                _volumeDownTimer.Stop();
                VolumeLevel = MinLevel;
            }

            UpdateVolume();

        }

        virtual public void Mute()
        {
            if (Muted)
            {
                VolumeLevel = _lastLevel;
                SymbolJoin.UShortValue = (ushort)VolumeType;
            }
            else
            {
                _lastLevel = VolumeLevel;
                SymbolJoin.UShortValue = (ushort)(VolumeType+1);
            }

            Muted = !Muted;
            UpdateVolume();
        }

    }
}
