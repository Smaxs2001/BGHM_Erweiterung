using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFE.HelpersX
{
    internal class CustomEventArgs
    {
        public class VolumeNamedControlEventArgs : EventArgs
        {
            public VolumeNamedControlEventArgs(string controlName, double position)
            {
                this.ControlName = controlName;
                this.Position = position;
            }
            public string ControlName { get; }

            public double Position { get; }
        }
    }
}
