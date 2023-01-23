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

        private System.Timers.Timer VolumeUpTimer;
        private System.Timers.Timer VolumeDownTimer;

        protected double MaxLevel = 65535;
        protected double MinLevel = 0;
        protected double VolumeLevel;
        private double lastLevel;
        protected bool muted = false;




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

            VolumeUpTimer = new System.Timers.Timer();
            VolumeUpTimer.Interval = 60;
            VolumeUpTimer.AutoReset = true;
            VolumeUpTimer.Elapsed += VolumeUp;

            VolumeDownTimer = new System.Timers.Timer();
            VolumeDownTimer.Interval = 60;
            VolumeDownTimer.AutoReset = true;
            VolumeDownTimer.Elapsed += VolumeDown;
        }

        public bool getmuted()
        {
            return muted;
        }
        virtual public void UpdateVolume()
        {
            VolumeGaugeJoin.UShortValue=(ushort)(VolumeLevel);
        }

        public void VolumeUpStart()
        {
            if (muted) return;
            VolumeUpTimer.Start();
        }

        public void VolumeUpStop()
        {
            VolumeUpTimer.Stop();
        }
        private void VolumeUp(object Sender,EventArgs args)
        {
            
            VolumeLevel = (VolumeLevel + Increment);
            if(VolumeLevel > MaxLevel-Increment)
            {
                VolumeUpTimer.Stop();
                VolumeLevel = MaxLevel;
            }

            UpdateVolume();

        }

        public void VolumeDownStart()
        {
            if (muted) return;
            VolumeDownTimer.Start();
        }

        public void VolumeDownStop()
        {
            VolumeDownTimer.Stop();
        }
        private void VolumeDown(object Sender, EventArgs args)
        {
            VolumeLevel = (VolumeLevel - Increment);
            if (VolumeLevel < MinLevel+Increment)
            {
                VolumeDownTimer.Stop();
                VolumeLevel = MinLevel;
            }

            UpdateVolume();

        }

        virtual public void mute()
        {
            if (muted)
            {
                VolumeLevel = lastLevel;
                SymbolJoin.UShortValue = (ushort)VolumeType;
            }
            else
            {
                lastLevel = VolumeLevel;
                SymbolJoin.UShortValue = (ushort)(VolumeType+1);
            }

            muted = !muted;
            UpdateVolume();
        }

    }
}
