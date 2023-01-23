using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.Diagnostics;

namespace BFE
{
    
    class Interlock
    {
        private int activeGroup = 0;
        private int lastActiveGroup = 0;

        List<List<BoolInputSig>> joins; 

        public Interlock() { 
            joins = new List<List<BoolInputSig>>();

        }

        public int GetActiveGroup() { return activeGroup; }

        public int GetLastActiveGroup() { return lastActiveGroup; }
        public void AddJoin(BoolInputSig join,int group)
        {
            joins[group].Add(join);
        }

        public int addGroup()
        {
            joins.Add(new List<BoolInputSig>());
            return joins.Count()-1;
        }


        public void activate(int Group)
        {
            try
            {
                CrestronConsole.PrintLine("Moin");
                lastActiveGroup = activeGroup;
                activeGroup = Group;

                foreach (List<BoolInputSig> list in joins)
                {
                    foreach (BoolInputSig input in list)
                    {
                        input.BoolValue = false;
                    }
                }
                CrestronConsole.PrintLine("Moin");

                foreach (BoolInputSig input in joins[Group])
                {
                    input.BoolValue = true;
                }
            }
            catch (Exception ex)
            {
                CrestronConsole.PrintLine("Error: {0}",ex.Message);
            }


        }
    }
}

