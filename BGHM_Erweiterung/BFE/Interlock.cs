using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace BFE
{
    
    class Interlock
    {
        private int _activeGroup;
        private int _lastActiveGroup;

        readonly List<List<BoolInputSig>> _joins; 

        public Interlock() { 
            _joins = new List<List<BoolInputSig>>();

        }

        public int GetActiveGroup() { return _activeGroup; }

        public int GetLastActiveGroup() { return _lastActiveGroup; }
        public void AddJoin(BoolInputSig join,int group)
        {
            _joins[group].Add(join);
        }

        public int AddGroup()
        {
            _joins.Add(new List<BoolInputSig>());
            return _joins.Count()-1;
        }


        public void Activate(int group)
        {
            try
            {
                CrestronConsole.PrintLine("Moin");
                _lastActiveGroup = _activeGroup;
                _activeGroup = group;

                foreach (List<BoolInputSig> list in _joins)
                {
                    foreach (BoolInputSig input in list)
                    {
                        input.BoolValue = false;
                    }
                }
                CrestronConsole.PrintLine("Moin");

                foreach (BoolInputSig input in _joins[group])
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

