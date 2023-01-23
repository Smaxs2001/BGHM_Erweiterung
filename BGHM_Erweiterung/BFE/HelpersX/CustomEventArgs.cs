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
            public VolumeNamedControlEventArgs(string controlName, double Position)
            {
                this.controlName = controlName;
                this.Position = Position;
            }
            public string controlName { get; }

            public double Position { get; }
        }
    }
}
