using Crestron.SimplSharpPro.DeviceSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFE
{
    class Passwort
    {
        string _pw;
        string _input;
        string _hiddenInput;
        Interlock _interlock;
        int _interlockGroup;
        Crestron.SimplSharpPro.StringInputSig _sternText;
        int _lastActiveGroup = 0;
        public Passwort(string passwort, Interlock interlock, int interlockGroup, Crestron.SimplSharpPro.StringInputSig sternText)
        {
            this._pw=passwort;
            this._sternText=sternText;
            this._interlock=interlock;
            this._interlockGroup = interlockGroup;
        }



        
        public void AddKeytoInput(char x)
        {
            _input = _input+ x;
            _hiddenInput = _hiddenInput + '*';
            _sternText.StringValue = _hiddenInput;
        } 
        

        public void CheckPassword()
        {
            if (_input == _pw)
            {
                _interlock.Activate(_interlockGroup);
            }
            ClearInput();

        }

        public void ClearInput()
        {
            _input ="";
            _hiddenInput = "";
            _sternText.StringValue = _hiddenInput;
        }

        public void Exit()
        {
            ClearInput();
            _interlock.Activate(_lastActiveGroup);
        }
    }
}
