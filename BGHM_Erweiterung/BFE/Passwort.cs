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
        string pw;
        string input;
        string hiddenInput;
        Interlock interlock;
        int InterlockGroup;
        Crestron.SimplSharpPro.StringInputSig sternText;
        int lastActiveGroup = 0;
        public Passwort(string passwort, Interlock interlock, int InterlockGroup, Crestron.SimplSharpPro.StringInputSig sternText)
        {
            this.pw=passwort;
            this.sternText=sternText;
            this.interlock=interlock;
            this.InterlockGroup = InterlockGroup;
        }



        
        public void addKeytoInput(char x)
        {
            input = input+ x;
            hiddenInput = hiddenInput + '*';
            sternText.StringValue = hiddenInput;
        } 
        

        public void checkPassword()
        {
            if (input == pw)
            {
                interlock.activate(InterlockGroup);
            }
            clearInput();

        }

        public void clearInput()
        {
            input ="";
            hiddenInput = "";
            sternText.StringValue = hiddenInput;
        }

        public void exit()
        {
            clearInput();
            interlock.activate(lastActiveGroup);
        }
    }
}
